using Api.Middleware;
using Flowingly.ParsingService.Api.Endpoints;
using Flowingly.ParsingService.Application.Validators;
using Flowingly.ParsingService.Application.Behaviors;
using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Services;
using Flowingly.ParsingService.Domain.Validation;
using Flowingly.ParsingService.Domain.Parsing;
using Flowingly.ParsingService.Domain.Parsers;
using Flowingly.ParsingService.Domain.Processors;
using Flowingly.ParsingService.Domain.Normalizers;
using Flowingly.ParsingService.Domain.Helpers;
using Flowingly.ParsingService.Infrastructure.Repositories;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Flowingly Parsing Service API",
        Version = "v1",
        Description = "Text ingestion and parsing service for extracting structured data from free-form text"
    });
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ParseRequestValidator>();

// Add MediatR with validation pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<ParseRequestValidator>();
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// Register Domain Services (Scoped - may have state during request processing)
builder.Services.AddScoped<ITagValidator, TagValidator>();
builder.Services.AddScoped<IXmlIslandExtractor, XmlIslandExtractor>();
builder.Services.AddScoped<ITaxCalculator, TaxCalculator>();
builder.Services.AddScoped<ITimeParser, TimeParser>();

// Register Domain Helpers & Normalizers (Singleton - pure functions, stateless)
builder.Services.AddSingleton<IRoundingHelper, RoundingHelper>();
builder.Services.AddSingleton<NumberNormalizer>();

// Register Content Router (Scoped - orchestrates processors per request)
builder.Services.AddScoped<ContentRouter>();

// Register Processors (Strategy pattern - both registered as IContentProcessor)
builder.Services.AddScoped<IContentProcessor, ExpenseProcessor>();
builder.Services.AddScoped<IContentProcessor, OtherProcessor>();

// Register Repositories (Scoped - per-request database context)
builder.Services.AddScoped<IExpenseRepository, InMemoryExpenseRepository>();

// Add CORS for local development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Exception mapping (run early, before routing and controllers)
app.UseMiddleware<ExceptionMappingMiddleware>();

// API Key Authentication (Production only, per ADR-0006)
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        var configuredKey = app.Configuration["Security:ApiKey"];

        // If configured key is empty/null, skip auth (allow deployment without key set yet)
        if (!string.IsNullOrEmpty(configuredKey) && apiKey != configuredKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ErrorCode = "UNAUTHORIZED",
                Message = "Invalid or missing API key",
                Details = (Dictionary<string, string>?)null
            });
            return;
        }

        await next();
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flowingly Parsing Service API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseHttpsRedirection();

// Map endpoints
app.MapParseEndpoint();

app.Run();

// Make Program class accessible to WebApplicationFactory for integration tests
public partial class Program { }

using Flowingly.ParsingService.Contracts.Errors;
using Flowingly.ParsingService.Contracts.Requests;
using Flowingly.ParsingService.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Flowingly.ParsingService.Api.Endpoints;

/// <summary>
/// Parse endpoint for text ingestion and processing
/// </summary>
public static class ParseEndpoint
{
    public static void MapParseEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/parse", (
            [FromBody] ParseRequest request,
            HttpContext httpContext) =>
        {
            // Generate correlation ID for traceability
            var correlationId = Guid.NewGuid().ToString();

            // M0 Echo Flow: Return mock expense response to verify frontend-backend integration
            // This will be replaced with actual parsing logic in M1
            var mockResponse = new
            {
                classification = "expense",
                expense = new
                {
                    vendor = "Mock Vendor",
                    description = "Sample expense for M0 testing",
                    total = 120.50m,
                    totalExclTax = 104.78m,
                    salesTax = 15.72m,
                    costCentre = "UNKNOWN",
                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                    time = (string?)null,
                    taxRate = 0.15m
                },
                meta = new
                {
                    correlationId,
                    processingTimeMs = 10,
                    warnings = new[] { "M0 echo flow - full parsing implemented in M1" }
                }
            };

            return Results.Ok(mockResponse);
        })
        .WithName("Parse")
        .WithTags("Parsing")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Parse raw text content";
            operation.Description = "Accepts raw text and extracts structured expense data or stores as other content";
            return operation;
        })
        .Produces<ParseResponseBase>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);
    }
}

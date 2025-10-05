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

            // Echo flow for M0: return the input as "other" content
            // Will be replaced with actual parsing logic in M1
            var response = new ParseResponse
            {
                CorrelationId = correlationId,
                Expense = null,
                Other = new OtherData
                {
                    RawContent = request.Content
                }
            };

            return Results.Ok(response);
        })
        .WithName("Parse")
        .WithTags("Parsing")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Parse raw text content";
            operation.Description = "Accepts raw text and extracts structured expense data or stores as other content";
            return operation;
        })
        .Produces<ParseResponse>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);
    }
}

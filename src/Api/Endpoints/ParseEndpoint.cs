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
            operation.Summary = "Parse text content for expense claims";
            operation.Description = @"Accepts free-form text with inline tags or XML islands, extracts expense data, validates required fields,
                computes NZ GST tax breakdown using Banker's Rounding (MidpointRounding.ToEven), and returns normalized JSON.
                Non-expense content is stored as Other/Unprocessed.

                **Request Examples:**

                1. Inline Tags:
                   ```
                   Hi Yvaine, Please create an expense claim.
                   <vendor>Mojo Coffee</vendor><total>120.50</total><payment_method>personal card</payment_method>
                   ```

                2. XML Island:
                   ```
                   Hi Yvaine, Please create an expense claim for the below.
                   <expense><cost_centre>DEV002</cost_centre><total>1024.01</total><payment_method>personal card</payment_method></expense>
                   ```

                **Response Classifications:**
                - `expense`: Successfully parsed expense claim with GST calculation
                - `other`: Non-expense content (stored as unprocessed)

                **Error Codes:**
                - `MISSING_TOTAL`: Expense claims must include a <total> tag
                - `UNCLOSED_TAGS`: Input contains unclosed or overlapping tags (stack-based validation)
                - `MALFORMED_XML`: Invalid XML structure in <expense> island

                **GST Calculation (Banker's Rounding):**
                - Default tax rate: 0.15 (NZ GST) unless specified in request
                - Total is tax-inclusive: total = excludingTax + salesTax
                - Rounding: MidpointRounding.ToEven (0.5 rounds to nearest even number)
                - Examples: 2.125 → 2.12, 2.135 → 2.14, 2.145 → 2.14";

            return operation;
        })
        .Produces<ParseResponseBase>(StatusCodes.Status200OK, "application/json", "Successful parsing (expense or other)")
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, "application/json", "Validation error (MISSING_TOTAL, UNCLOSED_TAGS, MALFORMED_XML)")
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, "application/json", "Internal server error");
    }
}

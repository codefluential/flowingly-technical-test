using Flowingly.ParsingService.Contracts.Errors;
using Flowingly.ParsingService.Domain.Exceptions;

namespace Api.Middleware;

/// <summary>
/// Global exception handling middleware that maps domain and validation exceptions
/// to standardized ErrorResponse DTOs with appropriate HTTP status codes.
/// </summary>
public class ExceptionMappingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMappingMiddleware> _logger;

    public ExceptionMappingMiddleware(RequestDelegate next, ILogger<ExceptionMappingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Get or generate correlation ID for traceability
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        // Determine HTTP status code based on exception type
        var statusCode = GetStatusCode(exception);

        // Log exception with correlation ID for troubleshooting
        _logger.LogError(
            exception,
            "Request failed with {StatusCode}. CorrelationId: {CorrelationId}, ExceptionType: {ExceptionType}",
            statusCode,
            correlationId,
            exception.GetType().Name
        );

        // Map exception to ErrorResponse
        var errorResponse = MapToErrorResponse(exception, correlationId);

        // Write JSON error response
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            // Domain validation errors (e.g., UNCLOSED_TAGS, MISSING_TOTAL)
            ValidationException => StatusCodes.Status400BadRequest,

            // FluentValidation errors (e.g., invalid taxRate range)
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,

            // Unhandled exceptions (internal server errors)
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static ErrorResponse MapToErrorResponse(Exception exception, string correlationId)
    {
        return exception switch
        {
            ValidationException validationEx => new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = validationEx.ErrorCode,
                Message = validationEx.Message,
                Details = validationEx.Data.Count > 0
                    ? validationEx.Data.Cast<System.Collections.DictionaryEntry>()
                        .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "")
                    : null
            },
            FluentValidation.ValidationException fluentEx => new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = "INVALID_REQUEST",
                Message = "Validation failed for one or more fields",
                Details = fluentEx.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
            },
            _ => new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = "INTERNAL_ERROR",
                Message = "An unexpected error occurred",
                Details = null
            }
        };
    }
}

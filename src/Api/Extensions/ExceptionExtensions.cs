using Api.Contracts;
using Flowingly.ParsingService.Domain.Constants;
using Flowingly.ParsingService.Domain.Exceptions;

namespace Api.Extensions;

/// <summary>
/// Extension methods for mapping exceptions to standardized ErrorResponse DTOs.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Maps a domain ValidationException to an ErrorResponse with the exception's error code preserved.
    /// </summary>
    /// <param name="exception">The ValidationException containing error code and message</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <returns>ErrorResponse with domain error code (e.g., UNCLOSED_TAGS, MISSING_TOTAL)</returns>
    public static ErrorResponse ToErrorResponse(this ValidationException exception, string correlationId)
    {
        // Extract additional details from exception Data dictionary if present
        object? details = null;
        if (exception.Data.Count > 0)
        {
            // Convert Data dictionary to anonymous object for JSON serialization
            var dataDict = new Dictionary<string, object?>();
            foreach (var key in exception.Data.Keys)
            {
                dataDict[key.ToString()!] = exception.Data[key];
            }
            details = dataDict;
        }

        return new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = exception.ErrorCode,
                Message = ErrorMessages.GetMessage(exception.ErrorCode),
                Details = details
            },
            CorrelationId = Guid.Parse(correlationId)
        };
    }

    /// <summary>
    /// Maps a FluentValidation.ValidationException to an ErrorResponse with INVALID_REQUEST code
    /// and field-level validation errors in details.
    /// </summary>
    /// <param name="exception">FluentValidation exception containing validation errors</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <returns>ErrorResponse with INVALID_REQUEST code and validation error details</returns>
    public static ErrorResponse ToErrorResponse(this FluentValidation.ValidationException exception, string correlationId)
    {
        // Extract field-level validation errors
        var errors = exception.Errors.Select(e => new
        {
            field = e.PropertyName,
            message = e.ErrorMessage
        }).ToArray();

        return new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = ErrorCodes.INVALID_REQUEST,
                Message = ErrorMessages.GetMessage(ErrorCodes.INVALID_REQUEST),
                Details = new { errors }
            },
            CorrelationId = Guid.Parse(correlationId)
        };
    }

    /// <summary>
    /// Maps an unhandled Exception to an ErrorResponse with INTERNAL_ERROR code.
    /// Does NOT include stack trace or sensitive details in response (logged server-side only).
    /// </summary>
    /// <param name="exception">Unhandled exception</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <returns>ErrorResponse with INTERNAL_ERROR code and support reference</returns>
    public static ErrorResponse ToErrorResponse(this Exception exception, string correlationId)
    {
        return new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = ErrorCodes.INTERNAL_ERROR,
                Message = ErrorMessages.GetMessage(ErrorCodes.INTERNAL_ERROR),
                Details = new
                {
                    reference = "Contact support with the correlation ID for assistance"
                }
            },
            CorrelationId = Guid.Parse(correlationId)
        };
    }
}

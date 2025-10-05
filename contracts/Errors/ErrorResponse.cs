namespace Flowingly.ParsingService.Contracts.Errors;

/// <summary>
/// Standard error response
/// </summary>
public sealed record ErrorResponse
{
    /// <summary>
    /// Correlation ID for traceability
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Error code (e.g., UNCLOSED_TAG, INVALID_XML, MISSING_TOTAL)
    /// </summary>
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Additional error details (optional)
    /// </summary>
    public Dictionary<string, string>? Details { get; init; }
}

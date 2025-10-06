namespace Api.Contracts;

/// <summary>
/// Detailed error information for failed parse requests.
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// Machine-readable error code (e.g., "VALIDATION_ERROR", "UNCLOSED_TAG").
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Human-readable error message describing what went wrong.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Optional additional details about the error (e.g., validation failures, stack trace in dev).
    /// </summary>
    public object? Details { get; set; }
}

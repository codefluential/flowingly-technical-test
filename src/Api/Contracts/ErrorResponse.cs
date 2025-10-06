namespace Api.Contracts;

/// <summary>
/// Response returned when parsing fails due to validation or processing errors.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Detailed error information including code, message, and optional details.
    /// </summary>
    public ErrorDetail Error { get; set; } = null!;

    /// <summary>
    /// Correlation identifier for tracing this error in logs.
    /// </summary>
    public Guid CorrelationId { get; set; }
}

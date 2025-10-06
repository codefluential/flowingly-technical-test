namespace Api.Contracts;

/// <summary>
/// Metadata included in all parse responses for traceability and diagnostics.
/// </summary>
public class ResponseMeta
{
    /// <summary>
    /// Unique correlation identifier for request tracing and logging.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// List of non-critical warnings encountered during parsing.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// List of tag names found in the parsed content.
    /// </summary>
    public List<string> TagsFound { get; set; } = new();
}

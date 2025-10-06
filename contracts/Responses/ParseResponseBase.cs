namespace Flowingly.ParsingService.Contracts.Responses;

/// <summary>
/// Abstract base class for all parse responses.
/// Enforces the XOR pattern: responses contain either expense OR other data, never both.
/// </summary>
public abstract class ParseResponseBase
{
    /// <summary>
    /// Classification of the parsed content: "expense" or "other".
    /// Determines which payload is present in the response.
    /// </summary>
    public abstract string Classification { get; }

    /// <summary>
    /// Response metadata including correlation ID, warnings, and tags found.
    /// </summary>
    public ResponseMeta Meta { get; set; } = null!;
}

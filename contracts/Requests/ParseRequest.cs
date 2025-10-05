namespace Flowingly.ParsingService.Contracts.Requests;

/// <summary>
/// Request to parse raw text containing inline tags and XML islands
/// </summary>
public sealed record ParseRequest
{
    /// <summary>
    /// Raw text content to parse (e.g., email body)
    /// </summary>
    public required string Content { get; init; }
}

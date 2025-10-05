namespace Flowingly.ParsingService.Contracts.Requests;

/// <summary>
/// Request to parse raw text containing inline tags and XML islands
/// </summary>
public sealed record ParseRequest
{
    /// <summary>
    /// Raw text content to parse (e.g., email body)
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Optional tax rate (defaults to 0.15 / NZ GST if not provided)
    /// </summary>
    public decimal? TaxRate { get; init; }
}

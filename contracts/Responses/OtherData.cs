namespace Flowingly.ParsingService.Contracts.Responses;

/// <summary>
/// Data for non-expense content that doesn't match known processors.
/// Stores raw tags for future processing.
/// </summary>
public class OtherData
{
    /// <summary>
    /// Dictionary of tag names to their values (e.g., reservation data).
    /// Preserved for future processor implementation.
    /// </summary>
    public Dictionary<string, string> RawTags { get; set; } = new();

    /// <summary>
    /// Human-readable note indicating this content is unprocessed.
    /// </summary>
    public string Note { get; set; } = null!;
}

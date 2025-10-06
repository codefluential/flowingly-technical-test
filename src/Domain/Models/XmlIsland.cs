namespace Flowingly.ParsingService.Domain.Models;

/// <summary>
/// Represents an extracted XML island from parsed content.
/// </summary>
public class XmlIsland
{
    /// <summary>
    /// Gets the name of the XML island (e.g., "expense", "reservation").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the XML content of the island.
    /// </summary>
    public string Content { get; init; } = string.Empty;
}

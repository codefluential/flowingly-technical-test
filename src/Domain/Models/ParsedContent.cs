namespace Flowingly.ParsingService.Domain.Models;

/// <summary>
/// Represents parsed content with inline tags and XML islands.
/// </summary>
public class ParsedContent
{
    /// <summary>
    /// Gets the dictionary of inline tags extracted from the content.
    /// Key is the tag name, value is the tag value.
    /// </summary>
    public Dictionary<string, string> InlineTags { get; init; } = new();

    /// <summary>
    /// Gets the list of XML islands extracted from the content.
    /// </summary>
    public List<XmlIsland> XmlIslands { get; init; } = new();

    /// <summary>
    /// Gets the original raw text content.
    /// </summary>
    public string RawText { get; init; } = string.Empty;
}

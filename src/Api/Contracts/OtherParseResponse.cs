namespace Api.Contracts;

/// <summary>
/// Response for content that doesn't match expense patterns.
/// Contains other/unprocessed data and metadata, but never expense data (XOR enforcement).
/// </summary>
public class OtherParseResponse : ParseResponseBase
{
    /// <summary>
    /// Classification is always "other" for this response type.
    /// </summary>
    public override string Classification => "other";

    /// <summary>
    /// Unprocessed content data including raw tags and explanatory note.
    /// </summary>
    public OtherData Other { get; set; } = null!;
}

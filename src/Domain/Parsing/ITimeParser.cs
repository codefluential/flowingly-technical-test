namespace Flowingly.ParsingService.Domain.Parsing;

/// <summary>
/// Interface for parsing time strings into TimeSpan values.
/// Supports 12-hour (AM/PM) and 24-hour formats with optional seconds.
/// </summary>
public interface ITimeParser
{
    /// <summary>
    /// Parses a time string into a TimeSpan.
    /// </summary>
    /// <param name="input">Time string (e.g., "14:30", "2:30 PM", "14:30:00")</param>
    /// <returns>TimeSpan if valid, null if invalid or ambiguous</returns>
    TimeSpan? Parse(string? input);
}

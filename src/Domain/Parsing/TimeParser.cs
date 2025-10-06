using System.Globalization;

namespace Flowingly.ParsingService.Domain.Parsing;

/// <summary>
/// Whitelist-based time parser supporting 12/24-hour formats.
/// Rejects ambiguous formats (e.g., "230", "2.30", "7.30pm").
/// </summary>
public class TimeParser : ITimeParser
{
    // Whitelist of accepted formats (ADR-0008)
    private static readonly string[] AcceptedFormats = new[]
    {
        "HH:mm",        // 24-hour without seconds (14:30)
        "HH:mm:ss",     // 24-hour with seconds (14:30:00)
        "h:mm tt",      // 12-hour without seconds (2:30 PM)
        "h:mm:ss tt"    // 12-hour with seconds (2:30:45 PM)
    };

    public TimeSpan? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var trimmed = input.Trim();

        // Try parsing as DateTime first (for 12-hour AM/PM formats)
        if (DateTime.TryParseExact(
            trimmed,
            AcceptedFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateTime))
        {
            return dateTime.TimeOfDay;
        }

        // Invalid or ambiguous format
        return null;
    }
}

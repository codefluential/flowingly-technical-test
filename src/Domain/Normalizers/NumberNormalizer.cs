using System.Globalization;
using System.Text.RegularExpressions;

namespace Flowingly.ParsingService.Domain.Normalizers;

/// <summary>
/// Normalizes number strings by removing currency symbols and thousand separators.
/// Returns decimal type for precision in financial calculations.
/// </summary>
public class NumberNormalizer
{
    private static readonly Regex CurrencySymbolRegex = new(@"[$£€¥₹]|NZD|USD|EUR|GBP|AUD", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public decimal Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Invalid input: Input cannot be null, empty, or whitespace. Provide a valid number string.", nameof(input));
        }

        // Remove leading/trailing whitespace
        var cleaned = input.Trim();

        // Remove currency symbols and codes
        cleaned = CurrencySymbolRegex.Replace(cleaned, string.Empty);

        // Remove whitespace after currency removal
        cleaned = cleaned.Trim();

        // Remove thousand separators (commas)
        cleaned = cleaned.Replace(",", string.Empty);

        // Parse as decimal with invariant culture
        if (decimal.TryParse(cleaned, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid input: '{input}' is not a valid number after normalization.", nameof(input));
    }
}

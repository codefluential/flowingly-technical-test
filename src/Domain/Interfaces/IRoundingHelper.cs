namespace Flowingly.ParsingService.Domain.Interfaces;

/// <summary>
/// Interface for rounding decimal values using Banker's Rounding (MidpointRounding.ToEven).
/// Used for GST tax calculations to ensure financial accuracy.
/// </summary>
public interface IRoundingHelper
{
    /// <summary>
    /// Rounds a decimal value to the specified precision using Banker's Rounding.
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <param name="decimals">Number of decimal places</param>
    /// <returns>Rounded decimal value</returns>
    decimal Round(decimal value, int decimals);
}

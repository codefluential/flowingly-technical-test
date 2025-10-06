using Flowingly.ParsingService.Domain.Interfaces;

namespace Flowingly.ParsingService.Domain.Helpers;

/// <summary>
/// Implements Banker's Rounding (MidpointRounding.ToEven) for financial calculations.
/// When a value is exactly at the midpoint (e.g., 2.125), it rounds to the nearest EVEN number.
/// </summary>
public class RoundingHelper : IRoundingHelper
{
    public decimal Round(decimal value, int decimals)
    {
        return Math.Round(value, decimals, MidpointRounding.ToEven);
    }
}

using Flowingly.ParsingService.Domain.Models;

namespace Flowingly.ParsingService.Domain.Interfaces;

/// <summary>
/// Interface for tax calculation services.
/// Calculates tax breakdown from tax-inclusive totals using NZ GST calculation.
/// </summary>
public interface ITaxCalculator
{
    /// <summary>
    /// Calculates tax breakdown from tax-inclusive total using NZ GST calculation.
    /// </summary>
    /// <param name="taxInclusive">Tax-inclusive total amount (must be non-negative)</param>
    /// <param name="taxRate">Tax rate as decimal (e.g., 0.15 for 15%). Default: 0.15 (NZ GST)</param>
    /// <returns>Tax calculation result with exclusive amount and GST</returns>
    /// <exception cref="ArgumentException">Thrown if taxInclusive is negative or taxRate is invalid</exception>
    TaxCalculationResult CalculateFromInclusive(decimal taxInclusive, decimal taxRate = 0.15m);
}

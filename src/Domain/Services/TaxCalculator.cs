using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Models;

namespace Flowingly.ParsingService.Domain.Services;

/// <summary>
/// Implements tax calculation services for NZ GST.
/// Uses Banker's Rounding (MidpointRounding.ToEven) for all monetary calculations.
/// </summary>
public class TaxCalculator : ITaxCalculator
{
    private readonly IRoundingHelper _roundingHelper;

    public TaxCalculator(IRoundingHelper roundingHelper)
    {
        _roundingHelper = roundingHelper;
    }

    public TaxCalculationResult CalculateFromInclusive(decimal taxInclusive, decimal taxRate = 0.15m)
    {
        // Validate inputs
        if (taxInclusive < 0)
            throw new ArgumentException("Tax-inclusive amount must be non-negative", nameof(taxInclusive));

        if (taxRate < 0 || taxRate >= 1)
            throw new ArgumentException("Tax rate must be between 0 and 1", nameof(taxRate));

        // Calculate tax-exclusive amount
        var taxExclusive = taxInclusive / (1 + taxRate);
        taxExclusive = _roundingHelper.Round(taxExclusive, 2);

        // Calculate GST
        var gst = taxInclusive - taxExclusive;
        gst = _roundingHelper.Round(gst, 2);

        return new TaxCalculationResult
        {
            TaxExclusive = taxExclusive,
            Gst = gst,
            TaxInclusive = taxInclusive,
            TaxRate = taxRate
        };
    }
}

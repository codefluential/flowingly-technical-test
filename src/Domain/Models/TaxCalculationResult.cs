namespace Flowingly.ParsingService.Domain.Models;

/// <summary>
/// Represents the result of a tax calculation.
/// </summary>
public class TaxCalculationResult
{
    /// <summary>
    /// Tax-exclusive amount (base amount before tax)
    /// </summary>
    public decimal TaxExclusive { get; init; }

    /// <summary>
    /// GST/sales tax amount
    /// </summary>
    public decimal Gst { get; init; }

    /// <summary>
    /// Tax-inclusive amount (total paid including tax)
    /// </summary>
    public decimal TaxInclusive { get; init; }

    /// <summary>
    /// Tax rate used in calculation (e.g., 0.15 for 15%)
    /// </summary>
    public decimal TaxRate { get; init; }
}

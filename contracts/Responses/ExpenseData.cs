namespace Flowingly.ParsingService.Contracts.Responses;

/// <summary>
/// Structured expense data extracted from parsed content.
/// Contains financial details including tax calculations.
/// </summary>
public class ExpenseData
{
    /// <summary>
    /// Vendor or merchant name (extracted from text or defaulted).
    /// </summary>
    public string Vendor { get; set; } = null!;

    /// <summary>
    /// Total amount including tax (tax-inclusive).
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Total amount excluding tax (calculated).
    /// </summary>
    public decimal TotalExclTax { get; set; }

    /// <summary>
    /// Sales tax amount (GST) calculated from inclusive total.
    /// </summary>
    public decimal SalesTax { get; set; }

    /// <summary>
    /// Cost centre code for expense allocation.
    /// Defaults to "UNKNOWN" if not provided.
    /// </summary>
    public string CostCentre { get; set; } = null!;

    /// <summary>
    /// Optional description or notes about the expense.
    /// </summary>
    public string? Description { get; set; }
}

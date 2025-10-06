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

    /// <summary>
    /// Payment method used (e.g., "personal card", "company card").
    /// </summary>
    public string PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Tax rate applied to this expense (e.g., 0.15 for 15% NZ GST).
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Currency code for the expense (e.g., "NZD", "USD"). Defaults to "NZD".
    /// </summary>
    public string Currency { get; set; } = "NZD";

    /// <summary>
    /// Source of the expense data (e.g., "expense-xml" for XML islands, "inline" for inline tags).
    /// </summary>
    public string Source { get; set; } = "inline";

    /// <summary>
    /// Date of the expense (if provided).
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// Time of the expense (if provided and successfully parsed).
    /// </summary>
    public string? Time { get; set; }
}

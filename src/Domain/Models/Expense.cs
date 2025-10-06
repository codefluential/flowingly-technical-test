namespace Flowingly.ParsingService.Domain.Models;

/// <summary>
/// Represents an expense domain entity with tax breakdown.
/// </summary>
public class Expense
{
    /// <summary>
    /// Gets or sets the unique identifier for the expense.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the vendor name.
    /// </summary>
    public string Vendor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expense description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total amount including tax.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Gets or sets the total amount excluding tax.
    /// </summary>
    public decimal TotalExclTax { get; set; }

    /// <summary>
    /// Gets or sets the sales tax/GST amount.
    /// </summary>
    public decimal SalesTax { get; set; }

    /// <summary>
    /// Gets or sets the cost centre. Defaults to "UNKNOWN" when not provided.
    /// </summary>
    public string CostCentre { get; set; } = "UNKNOWN";

    /// <summary>
    /// Gets or sets the expense date.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expense time.
    /// </summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax rate used for calculation (e.g., 0.15 for 15% NZ GST).
    /// </summary>
    public decimal TaxRate { get; set; } = 0.15m;
}

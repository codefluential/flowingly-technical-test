namespace Flowingly.ParsingService.Contracts.Responses;

/// <summary>
/// Response from parsing operation
/// </summary>
public sealed record ParseResponse
{
    /// <summary>
    /// Unique correlation ID for traceability
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Expense data (populated if content is expense-related)
    /// </summary>
    public ExpenseData? Expense { get; init; }

    /// <summary>
    /// Other/unprocessed data (populated if content is not expense-related)
    /// </summary>
    public OtherData? Other { get; init; }
}

/// <summary>
/// Expense data extracted from content
/// </summary>
public sealed record ExpenseData
{
    /// <summary>
    /// Cost centre code (defaults to UNKNOWN if not provided)
    /// </summary>
    public required string CostCentre { get; init; }

    /// <summary>
    /// Tax-inclusive total amount
    /// </summary>
    public required decimal Total { get; init; }

    /// <summary>
    /// Tax amount (calculated from total using GST rate)
    /// </summary>
    public required decimal TaxAmount { get; init; }

    /// <summary>
    /// Tax-exclusive total (total - tax)
    /// </summary>
    public required decimal TotalExcludingTax { get; init; }

    /// <summary>
    /// Currency code (defaults to NZD)
    /// </summary>
    public required string Currency { get; init; }
}

/// <summary>
/// Other/unprocessed content data
/// </summary>
public sealed record OtherData
{
    /// <summary>
    /// Original raw content for future processing
    /// </summary>
    public required string RawContent { get; init; }
}

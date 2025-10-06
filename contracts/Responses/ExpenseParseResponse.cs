namespace Flowingly.ParsingService.Contracts.Responses;

/// <summary>
/// Response for successfully parsed expense content.
/// Contains expense data and metadata, but never "other" data (XOR enforcement).
/// </summary>
public class ExpenseParseResponse : ParseResponseBase
{
    /// <summary>
    /// Classification is always "expense" for this response type.
    /// </summary>
    public override string Classification => "expense";

    /// <summary>
    /// Structured expense data including financial details and tax calculations.
    /// </summary>
    public ExpenseData Expense { get; set; } = null!;
}

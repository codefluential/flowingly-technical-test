using System.Text.RegularExpressions;
using Flowingly.ParsingService.Domain.Exceptions;
using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Models;
using Flowingly.ParsingService.Domain.Normalizers;
using Flowingly.ParsingService.Domain.Parsing;

namespace Flowingly.ParsingService.Domain.Processors;

/// <summary>
/// Processes expense content through a 5-stage pipeline:
/// 1. Validate - Check required fields
/// 2. Extract - Pull expense fields from tags/islands
/// 3. Normalize - Calculate tax breakdown
/// 4. Persist - Save to repository
/// 5. BuildResponse - Create processing result
/// </summary>
public class ExpenseProcessor : IContentProcessor
{
    private readonly ITaxCalculator _taxCalculator;
    private readonly IExpenseRepository _repository;
    private readonly NumberNormalizer _numberNormalizer;
    private readonly ITimeParser _timeParser;

    public string ContentType => "expense";

    public ExpenseProcessor(
        ITaxCalculator taxCalculator,
        IExpenseRepository repository,
        NumberNormalizer numberNormalizer,
        ITimeParser timeParser)
    {
        _taxCalculator = taxCalculator ?? throw new ArgumentNullException(nameof(taxCalculator));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _numberNormalizer = numberNormalizer ?? throw new ArgumentNullException(nameof(numberNormalizer));
        _timeParser = timeParser ?? throw new ArgumentNullException(nameof(timeParser));
    }

    public bool CanProcess(ParsedContent content)
    {
        // Can process if any expense-related tags exist OR <expense> XML island exists
        // Expense-related tags: total, vendor, cost_centre, payment_method, description, date, time
        var expenseTags = new[] { "total", "vendor", "cost_centre", "payment_method", "description", "date", "time" };

        bool hasExpenseTag = content.InlineTags.Keys.Any(tag => expenseTags.Contains(tag));
        bool hasExpenseIsland = content.XmlIslands.Any(x => x.Name == "expense");

        return hasExpenseTag || hasExpenseIsland;
    }

    public async Task<ProcessingResult> ProcessAsync(ParsedContent content, CancellationToken ct)
    {
        var warnings = new List<string>();

        // Stage 1: Validate
        ValidateRequiredFields(content);

        // Stage 2: Extract
        var expense = ExtractExpense(content, warnings);

        // Stage 3: Normalize (tax calculation)
        var taxRate = content.TaxRate; // Use tax rate from request (defaults to 0.15)
        var taxResult = _taxCalculator.CalculateFromInclusive(expense.Total, taxRate);
        expense.TotalExclTax = taxResult.TaxExclusive;
        expense.SalesTax = taxResult.Gst;
        expense.TaxRate = taxRate;

        // Stage 4: Persist
        await _repository.SaveAsync(expense, ct);

        // Stage 5: Build response
        return new ProcessingResult
        {
            Classification = "expense",
            Data = expense,
            Success = true,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Stage 1: Validates that required fields are present.
    /// </summary>
    private void ValidateRequiredFields(ParsedContent content)
    {
        // Check for <total> in inline tags OR in <expense> XML island
        bool hasTotalInTags = content.InlineTags.ContainsKey("total");
        bool hasTotalInIsland = content.XmlIslands
            .Where(x => x.Name == "expense")
            .Any(x => x.Content.Contains("<total>"));

        if (!hasTotalInTags && !hasTotalInIsland)
        {
            throw new ValidationException("MISSING_TOTAL", "MISSING_TOTAL: <total> tag is required for expense processing");
        }
    }

    /// <summary>
    /// Stage 2: Extracts expense fields from inline tags and XML islands.
    /// XML island data takes precedence for total and cost_centre.
    /// </summary>
    private Expense ExtractExpense(ParsedContent content, List<string> warnings)
    {
        var expense = new Expense();

        // Extract from inline tags first
        if (content.InlineTags.TryGetValue("vendor", out var vendor))
            expense.Vendor = vendor;
        if (content.InlineTags.TryGetValue("description", out var description))
            expense.Description = description;
        if (content.InlineTags.TryGetValue("total", out var total))
            expense.Total = _numberNormalizer.Normalize(total);
        if (content.InlineTags.TryGetValue("cost_centre", out var costCentre))
            expense.CostCentre = costCentre;
        if (content.InlineTags.TryGetValue("date", out var date))
            expense.Date = date;
        if (content.InlineTags.TryGetValue("time", out var timeStr))
        {
            var parsedTime = _timeParser.Parse(timeStr);
            if (parsedTime.HasValue)
            {
                expense.Time = parsedTime.Value.ToString(@"hh\:mm");
            }
            else
            {
                warnings.Add($"Time value '{timeStr}' could not be parsed and was ignored");
            }
        }
        if (content.InlineTags.TryGetValue("payment_method", out var paymentMethod))
            expense.PaymentMethod = paymentMethod;

        // Override with XML island data (takes precedence for total/cost_centre)
        var expenseIsland = content.XmlIslands.FirstOrDefault(x => x.Name == "expense");
        if (expenseIsland != null)
        {
            // Set source to indicate XML island origin
            expense.Source = "expense-xml";

            // Extract total from XML island
            if (expenseIsland.Content.Contains("<total>"))
            {
                var totalMatch = Regex.Match(expenseIsland.Content, @"<total>(.*?)</total>");
                if (totalMatch.Success)
                    expense.Total = _numberNormalizer.Normalize(totalMatch.Groups[1].Value);
            }

            // Extract cost_centre from XML island
            if (expenseIsland.Content.Contains("<cost_centre>"))
            {
                var costCentreMatch = Regex.Match(expenseIsland.Content, @"<cost_centre>(.*?)</cost_centre>");
                if (costCentreMatch.Success)
                    expense.CostCentre = costCentreMatch.Groups[1].Value;
            }

            // Extract payment_method from XML island
            if (expenseIsland.Content.Contains("<payment_method>"))
            {
                var paymentMethodMatch = Regex.Match(expenseIsland.Content, @"<payment_method>(.*?)</payment_method>");
                if (paymentMethodMatch.Success)
                    expense.PaymentMethod = paymentMethodMatch.Groups[1].Value;
            }
        }

        // Set currency from content (passed from handler)
        expense.Currency = content.Currency;

        // Set tax rate from content (passed from handler)
        expense.TaxRate = content.TaxRate;

        // Default cost_centre to 'UNKNOWN' if not provided
        if (string.IsNullOrEmpty(expense.CostCentre))
            expense.CostCentre = "UNKNOWN";

        return expense;
    }
}

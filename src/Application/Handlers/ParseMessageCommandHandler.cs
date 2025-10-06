using System.Text.RegularExpressions;
using Flowingly.ParsingService.Contracts.Responses;
using Flowingly.ParsingService.Application.Commands;
using Flowingly.ParsingService.Domain.Models;
using Flowingly.ParsingService.Domain.Parsers;
using Flowingly.ParsingService.Domain.Services;
using Flowingly.ParsingService.Domain.Validation;
using Flowingly.ParsingService.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flowingly.ParsingService.Application.Handlers;

/// <summary>
/// CQRS Command Handler for ParseMessageCommand.
/// Orchestrates the parse flow: validate → extract → route → process → build response.
/// Generates correlation ID, applies defaults, and maps domain results to API DTOs.
/// </summary>
public class ParseMessageCommandHandler : IRequestHandler<ParseMessageCommand, ParseResponseBase>
{
    private readonly ITagValidator _tagValidator;
    private readonly IXmlIslandExtractor _xmlExtractor;
    private readonly ContentRouter _router;
    private readonly ILogger<ParseMessageCommandHandler> _logger;
    private readonly IConfiguration _configuration;

    // Regex for extracting inline tags like <total>123</total>
    private static readonly Regex InlineTagRegex = new(@"<(\w+)>([^<]+)</\1>", RegexOptions.Compiled);

    public ParseMessageCommandHandler(
        ITagValidator tagValidator,
        IXmlIslandExtractor xmlExtractor,
        ContentRouter router,
        ILogger<ParseMessageCommandHandler> logger,
        IConfiguration configuration)
    {
        _tagValidator = tagValidator ?? throw new ArgumentNullException(nameof(tagValidator));
        _xmlExtractor = xmlExtractor ?? throw new ArgumentNullException(nameof(xmlExtractor));
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<ParseResponseBase> Handle(ParseMessageCommand request, CancellationToken cancellationToken)
    {
        // Start timing the request
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Generate correlation ID at handler level (not endpoint)
        var correlationId = Guid.NewGuid();

        // Apply defaults with config precedence (PRD Section 4.2)
        var taxRate = DetermineTaxRate(request.TaxRate);
        var currency = request.Currency ?? _configuration["Parsing:DefaultCurrency"] ?? "NZD";

        _logger.LogInformation(
            "Processing parse request. CorrelationId: {CorrelationId}, TaxRate: {TaxRate}, Currency: {Currency}",
            correlationId, taxRate, currency);

        // Stage 1: Validate tag integrity (throws ValidationException on failure)
        _tagValidator.Validate(request.Text);

        // Stage 2: Extract XML islands
        var xmlIslands = _xmlExtractor.Extract(request.Text)
            .Select(xml => new XmlIsland { Name = "expense", Content = xml })
            .ToList();

        // Stage 3: Extract inline tags
        var inlineTags = ExtractInlineTags(request.Text);

        // Stage 4: Build ParsedContent
        var parsedContent = new ParsedContent
        {
            InlineTags = inlineTags,
            XmlIslands = xmlIslands,
            RawText = request.Text,
            TaxRate = taxRate,
            Currency = currency
        };

        // Stage 5: Route to appropriate processor and get result
        var processingResult = await _router.RouteAsync(parsedContent, cancellationToken);

        // Stage 6: Build response based on classification (XOR enforcement)
        stopwatch.Stop();
        var response = BuildResponse(processingResult, correlationId, inlineTags.Keys.ToList(), stopwatch);

        _logger.LogInformation(
            "Parse request completed. CorrelationId: {CorrelationId}, Classification: {Classification}, ProcessingTime: {ProcessingTimeMs}ms",
            correlationId, processingResult.Classification, stopwatch.ElapsedMilliseconds);

        return response;
    }

    /// <summary>
    /// Extracts inline tags from text using regex pattern matching.
    /// Example: "&lt;total&gt;123&lt;/total&gt;" → { "total": "123" }
    /// </summary>
    private Dictionary<string, string> ExtractInlineTags(string text)
    {
        var tags = new Dictionary<string, string>();
        var matches = InlineTagRegex.Matches(text);

        foreach (Match match in matches)
        {
            var tagName = match.Groups[1].Value;
            var tagValue = match.Groups[2].Value.Trim();

            // Store last occurrence if duplicate tags exist
            tags[tagName] = tagValue;
        }

        return tags;
    }

    /// <summary>
    /// Maps domain ProcessingResult to API response DTO (expense XOR other).
    /// Ensures XOR enforcement: response contains EITHER expense OR other, NEVER both.
    /// </summary>
    private ParseResponseBase BuildResponse(
        ProcessingResult result,
        Guid correlationId,
        List<string> tagsFound,
        System.Diagnostics.Stopwatch stopwatch)
    {
        var meta = new ResponseMeta
        {
            CorrelationId = correlationId,
            Warnings = result.Warnings ?? new List<string>(),
            TagsFound = tagsFound,
            ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
        };

        // XOR enforcement: return expense OR other based on classification
        if (result.Classification == "expense")
        {
            var expense = result.Data as Expense
                ?? throw new InvalidOperationException("Expected Expense data for expense classification");

            return new ExpenseParseResponse
            {
                Expense = new ExpenseData
                {
                    Vendor = expense.Vendor,
                    Total = expense.Total,
                    TotalExclTax = expense.TotalExclTax,
                    SalesTax = expense.SalesTax,
                    CostCentre = expense.CostCentre,
                    Description = expense.Description,
                    PaymentMethod = expense.PaymentMethod,
                    TaxRate = expense.TaxRate,
                    Currency = expense.Currency,
                    Source = expense.Source,
                    Date = string.IsNullOrEmpty(expense.Date) ? null : expense.Date,
                    Time = string.IsNullOrEmpty(expense.Time) ? null : expense.Time
                },
                Meta = meta
            };
        }
        else // classification == "other"
        {
            // OtherProcessor returns anonymous object with RawTags and Note
            var otherData = result.Data
                ?? throw new InvalidOperationException("Expected data for other classification");

            // Use reflection to extract RawTags and Note from anonymous type
            var rawTagsProperty = otherData.GetType().GetProperty("RawTags");
            var noteProperty = otherData.GetType().GetProperty("Note");

            var rawTags = (Dictionary<string, string>)(rawTagsProperty?.GetValue(otherData)
                ?? new Dictionary<string, string>());
            var note = (string)(noteProperty?.GetValue(otherData)
                ?? "Content stored for future processing");

            return new OtherParseResponse
            {
                Other = new OtherData
                {
                    RawTags = rawTags,
                    Note = note
                },
                Meta = meta
            };
        }
    }

    /// <summary>
    /// Determines tax rate using precedence logic per PRD Section 4.2:
    /// 1. Request taxRate (wins if present)
    /// 2. Config Parsing:DefaultTaxRate
    /// 3. If both null + StrictTaxRate=true → throw MISSING_TAXRATE
    /// 4. If both null + StrictTaxRate=false → fallback to 0.15
    /// </summary>
    private decimal DetermineTaxRate(decimal? requestTaxRate)
    {
        // 1. Request tax rate wins if present
        if (requestTaxRate.HasValue)
            return requestTaxRate.Value;

        // 2. Try config default
        var configTaxRateStr = _configuration["Parsing:DefaultTaxRate"];
        if (decimal.TryParse(configTaxRateStr, out var configTaxRate))
            return configTaxRate;

        // 3. Both null - check strict mode
        var strictMode = _configuration.GetValue<bool>("Parsing:StrictTaxRate");
        if (strictMode)
        {
            throw new ValidationException("MISSING_TAXRATE",
                "MISSING_TAXRATE: Tax rate is required but not provided in request or configuration");
        }

        // 4. Fallback to 0.15 (NZ GST)
        return 0.15m;
    }
}

namespace Flowingly.ParsingService.Domain.Constants;

/// <summary>
/// Immutable error codes for API responses.
/// NEVER change these values after release - API stability requirement.
/// </summary>
public static class ErrorCodes
{
    // HTTP 400 - Client Errors

    /// <summary>
    /// Text contains unclosed or improperly closed tags.
    /// </summary>
    public const string UNCLOSED_TAGS = "UNCLOSED_TAGS";

    /// <summary>
    /// Text contains malformed or overlapping tags.
    /// </summary>
    public const string MALFORMED_TAGS = "MALFORMED_TAGS";

    /// <summary>
    /// Expense content is missing the required &lt;total&gt; tag.
    /// </summary>
    public const string MISSING_TOTAL = "MISSING_TOTAL";

    /// <summary>
    /// Text is null, empty, or contains only whitespace.
    /// </summary>
    public const string EMPTY_TEXT = "EMPTY_TEXT";

    /// <summary>
    /// Request validation failed (FluentValidation failures).
    /// </summary>
    public const string INVALID_REQUEST = "INVALID_REQUEST";

    /// <summary>
    /// Tax rate is required when StrictTaxRate mode is enabled.
    /// </summary>
    public const string MISSING_TAXRATE = "MISSING_TAXRATE";

    // HTTP 500 - Server Errors

    /// <summary>
    /// An unexpected error occurred during request processing.
    /// </summary>
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
}

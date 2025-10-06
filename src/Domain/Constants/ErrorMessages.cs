namespace Flowingly.ParsingService.Domain.Constants;

/// <summary>
/// Human-friendly error messages mapped to error codes.
/// </summary>
public static class ErrorMessages
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        {
            ErrorCodes.UNCLOSED_TAGS,
            "Text contains unclosed or improperly closed tags. All tags must be properly opened and closed."
        },
        {
            ErrorCodes.MALFORMED_TAGS,
            "Text contains malformed or overlapping tags. Ensure tags are properly nested."
        },
        {
            ErrorCodes.MISSING_TOTAL,
            "Expense content must include a <total> tag with the tax-inclusive amount."
        },
        {
            ErrorCodes.EMPTY_TEXT,
            "Text cannot be empty or whitespace. Please provide content to parse."
        },
        {
            ErrorCodes.INVALID_REQUEST,
            "Request validation failed. Check the details for specific field errors."
        },
        {
            ErrorCodes.MISSING_TAXRATE,
            "Tax rate is required when StrictTaxRate is enabled."
        },
        {
            ErrorCodes.INTERNAL_ERROR,
            "An unexpected error occurred while processing your request. Please try again or contact support."
        }
    };

    /// <summary>
    /// Gets the human-friendly message for an error code.
    /// </summary>
    /// <param name="errorCode">The error code constant.</param>
    /// <returns>Human-readable error message.</returns>
    public static string GetMessage(string errorCode)
    {
        return Messages.TryGetValue(errorCode, out var message)
            ? message
            : "An error occurred while processing your request.";
    }
}

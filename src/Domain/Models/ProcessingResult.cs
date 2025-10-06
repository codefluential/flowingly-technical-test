namespace Flowingly.ParsingService.Domain.Models;

/// <summary>
/// Represents the result of content processing.
/// </summary>
public class ProcessingResult
{
    /// <summary>
    /// Gets the classification of the content (e.g., "expense", "other").
    /// </summary>
    public string Classification { get; init; } = string.Empty;

    /// <summary>
    /// Gets the processed data. The type varies based on classification.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Gets a value indicating whether the processing was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the error code if processing failed.
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;
}

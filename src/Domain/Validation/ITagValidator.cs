namespace Flowingly.ParsingService.Domain.Validation;

/// <summary>
/// Interface for tag validation services.
/// Validates tag integrity (balanced, properly nested) in text content.
/// </summary>
public interface ITagValidator
{
    /// <summary>
    /// Validates that all tags in the input are properly closed and nested.
    /// </summary>
    /// <param name="input">The content to validate.</param>
    /// <exception cref="Domain.Exceptions.ValidationException">
    /// Thrown when unclosed or overlapping tags are detected.
    /// Error code: UNCLOSED_TAGS
    /// </exception>
    void Validate(string input);
}

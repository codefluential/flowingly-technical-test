using System.Text.RegularExpressions;
using Flowingly.ParsingService.Domain.Exceptions;

namespace Flowingly.ParsingService.Domain.Validation;

/// <summary>
/// Stack-based tag validator for detecting unclosed and overlapping tags.
/// Uses LIFO (Last-In-First-Out) stack to match opening/closing tag pairs.
/// </summary>
public class TagValidator : ITagValidator
{
    private const string ErrorCode = "UNCLOSED_TAGS";
    private static readonly Regex TagRegex = new(@"<(/?)(\w+)[^>]*>", RegexOptions.Compiled);

    /// <summary>
    /// Validates that all tags in the input are properly closed and nested.
    /// Uses a stack-based algorithm to detect overlapping tags.
    /// </summary>
    /// <param name="input">The content to validate.</param>
    /// <exception cref="ValidationException">
    /// Thrown when unclosed or overlapping tags are detected.
    /// Error code: UNCLOSED_TAGS
    /// </exception>
    public void Validate(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return; // Empty input is valid
        }

        var stack = new Stack<string>();
        var matches = TagRegex.Matches(input);

        foreach (Match match in matches)
        {
            var isClosing = match.Groups[1].Value == "/";
            var tagName = match.Groups[2].Value;

            if (isClosing)
            {
                ValidateClosingTag(stack, tagName);
            }
            else
            {
                stack.Push(tagName);
            }
        }

        ValidateAllTagsClosed(stack);
    }

    /// <summary>
    /// Validates that a closing tag has a matching opening tag on the stack.
    /// </summary>
    private static void ValidateClosingTag(Stack<string> stack, string tagName)
    {
        if (stack.Count == 0 || stack.Peek() != tagName)
        {
            throw new ValidationException(
                ErrorCode,
                FormatMismatchedTagError(tagName));
        }
        stack.Pop();
    }

    /// <summary>
    /// Validates that all opening tags have been closed.
    /// </summary>
    private static void ValidateAllTagsClosed(Stack<string> stack)
    {
        if (stack.Count > 0)
        {
            throw new ValidationException(
                ErrorCode,
                FormatUnclosedTagsError(stack));
        }
    }

    /// <summary>
    /// Formats error message for mismatched closing tag.
    /// </summary>
    private static string FormatMismatchedTagError(string tagName)
    {
        return $"{ErrorCode}: Tag validation failed - unclosed or mismatched tags detected. " +
               $"Found closing tag </{tagName}> without matching opening tag.";
    }

    /// <summary>
    /// Formats error message for unclosed tags.
    /// </summary>
    private static string FormatUnclosedTagsError(Stack<string> stack)
    {
        return $"{ErrorCode}: Tag validation failed - {stack.Count} unclosed tag(s) detected: " +
               $"{string.Join(", ", stack)}";
    }
}

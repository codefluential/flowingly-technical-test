using System.Text.RegularExpressions;
using Flowingly.ParsingService.Domain.Exceptions;

namespace Flowingly.ParsingService.Domain.Validation;

/// <summary>
/// Stack-based tag validator for detecting unclosed and overlapping tags.
/// Uses LIFO (Last-In-First-Out) stack to match opening/closing tag pairs.
/// </summary>
public class TagValidator
{
    private static readonly Regex TagRegex = new(@"<(/?)(\w+)[^>]*>", RegexOptions.Compiled);

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
                // Closing tag
                if (stack.Count == 0 || stack.Peek() != tagName)
                {
                    throw new ValidationException(
                        $"UNCLOSED_TAGS: Tag validation failed - unclosed or mismatched tags detected. Found closing tag </{tagName}> without matching opening tag.",
                        "UNCLOSED_TAGS");
                }
                stack.Pop();
            }
            else
            {
                // Opening tag
                stack.Push(tagName);
            }
        }

        // If stack not empty, there are unclosed tags
        if (stack.Count > 0)
        {
            throw new ValidationException(
                $"UNCLOSED_TAGS: Tag validation failed - {stack.Count} unclosed tag(s) detected: {string.Join(", ", stack)}",
                "UNCLOSED_TAGS");
        }
    }
}

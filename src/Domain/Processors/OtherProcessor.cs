using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Models;

namespace Flowingly.ParsingService.Domain.Processors;

/// <summary>
/// Processes non-expense content (fallback processor).
/// Stores raw tags for future processing by other processors (e.g., reservation).
/// </summary>
public class OtherProcessor : IContentProcessor
{
    public string ContentType => "other";

    /// <summary>
    /// OtherProcessor is a fallback - it can process any content.
    /// In practice, this is selected when no other processor claims the content.
    /// </summary>
    public bool CanProcess(ParsedContent content)
    {
        // Fallback processor - always returns true
        return true;
    }

    public Task<ProcessingResult> ProcessAsync(ParsedContent content, CancellationToken ct)
    {
        // Store all inline tags for future processing
        var result = new ProcessingResult
        {
            Classification = "other",
            Data = new
            {
                RawTags = content.InlineTags,
                Note = "Content stored for future processing"
            },
            Success = true
        };

        return Task.FromResult(result);
    }
}

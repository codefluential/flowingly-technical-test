using Flowingly.ParsingService.Domain.Models;

namespace Flowingly.ParsingService.Domain.Interfaces;

/// <summary>
/// Strategy interface for content processors.
/// Each processor implements this interface to handle specific content types.
/// </summary>
public interface IContentProcessor
{
    /// <summary>
    /// Gets the content type this processor handles (e.g., "expense", "other").
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Determines whether this processor can handle the given parsed content.
    /// </summary>
    /// <param name="content">The parsed content to evaluate.</param>
    /// <returns>True if this processor can handle the content; otherwise, false.</returns>
    bool CanProcess(ParsedContent content);

    /// <summary>
    /// Processes the parsed content asynchronously.
    /// </summary>
    /// <param name="content">The parsed content to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The processing result.</returns>
    Task<ProcessingResult> ProcessAsync(ParsedContent content, CancellationToken ct);
}

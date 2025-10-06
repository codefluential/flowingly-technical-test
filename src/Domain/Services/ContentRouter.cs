using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Models;

namespace Flowingly.ParsingService.Domain.Services;

/// <summary>
/// Routes parsed content to the appropriate processor using the Strategy pattern.
/// </summary>
public class ContentRouter
{
    private readonly IEnumerable<IContentProcessor> _processors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentRouter"/> class.
    /// </summary>
    /// <param name="processors">The collection of registered content processors.</param>
    public ContentRouter(IEnumerable<IContentProcessor> processors)
    {
        _processors = processors;
    }

    /// <summary>
    /// Routes the parsed content to the appropriate processor.
    /// </summary>
    /// <param name="content">The parsed content to route.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The processing result from the selected processor.</returns>
    public async Task<ProcessingResult> RouteAsync(ParsedContent content, CancellationToken ct)
    {
        // Select first processor that can handle the content
        var processor = _processors.FirstOrDefault(p => p.CanProcess(content))
                        ?? _processors.First(p => p.ContentType == "other");

        // Delegate to selected processor
        return await processor.ProcessAsync(content, ct);
    }
}

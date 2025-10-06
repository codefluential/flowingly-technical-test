using Flowingly.ParsingService.Contracts.Responses;
using MediatR;

namespace Flowingly.ParsingService.Application.Commands;

/// <summary>
/// Command to parse message text and extract structured data.
/// Contains request parameters for parsing operation.
/// </summary>
/// <param name="Text">The raw text content to parse (required).</param>
/// <param name="TaxRate">Optional tax rate override (defaults to 0.15 if not provided).</param>
/// <param name="Currency">Optional currency code (defaults to "NZD" if not provided).</param>
public record ParseMessageCommand(
    string Text,
    decimal? TaxRate = null,
    string? Currency = null
) : IRequest<ParseResponseBase>;

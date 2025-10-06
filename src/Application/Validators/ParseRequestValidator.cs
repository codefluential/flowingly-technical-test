using FluentValidation;
using Flowingly.ParsingService.Contracts.Requests;

namespace Flowingly.ParsingService.Application.Validators;

/// <summary>
/// Validator for ParseRequest with comprehensive validation rules.
/// Ensures text content is valid and tax rate is within acceptable range.
/// </summary>
public class ParseRequestValidator : AbstractValidator<ParseRequest>
{
    /// <summary>
    /// Maximum allowed text length: 256KB (262144 bytes).
    /// </summary>
    private const int MaxTextLength = 262144;

    public ParseRequestValidator()
    {
        // Text validation: required and cannot exceed 256KB
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text is required and cannot be empty")
            .MaximumLength(MaxTextLength)
            .WithMessage($"Text cannot exceed 256KB ({MaxTextLength} bytes)");

        // TaxRate validation: optional but must be between 0 and 1 when provided
        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0m, 1m)
            .When(x => x.TaxRate.HasValue)
            .WithMessage("TaxRate must be between 0 and 1 when provided");
    }
}

using System.ComponentModel.DataAnnotations;

namespace Api.Contracts;

/// <summary>
/// Request payload for parsing free-form text containing inline tags and XML islands.
/// </summary>
public class ParseRequest
{
    /// <summary>
    /// The raw text content to be parsed (e.g., email body with inline tags).
    /// </summary>
    [Required(ErrorMessage = "Text content is required")]
    public string Text { get; set; } = null!;

    /// <summary>
    /// Optional tax rate for calculations (0.0 to 1.0).
    /// If not provided, defaults to 0.15 (NZ GST rate).
    /// </summary>
    [Range(0, 1, ErrorMessage = "Tax rate must be between 0 and 1")]
    public decimal? TaxRate { get; set; }
}

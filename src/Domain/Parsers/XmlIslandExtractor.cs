using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Flowingly.ParsingService.Domain.Parsers;

/// <summary>
/// Secure XML island extractor with DTD/XXE/DoS protection.
/// Extracts &lt;expense&gt;...&lt;/expense&gt; blocks from free-form text.
/// </summary>
public class XmlIslandExtractor : IXmlIslandExtractor
{
    private const int MaxInputSizeBytes = 2_000_000; // 2MB limit for DoS protection
    private static readonly Regex ExpenseIslandRegex = new(@"<expense\b[^>]*>.*?</expense>", RegexOptions.Compiled | RegexOptions.Singleline);

    public IEnumerable<string> Extract(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Enumerable.Empty<string>();
        }

        // Security: Check for DTD declarations in input (before extraction)
        if (input.Contains("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
            input.Contains("<!ENTITY", StringComparison.OrdinalIgnoreCase))
        {
            throw new XmlException("DTD declarations are prohibited for security reasons.");
        }

        // DoS protection: reject overly large inputs
        if (System.Text.Encoding.UTF8.GetByteCount(input) > MaxInputSizeBytes)
        {
            throw new ArgumentException($"Input size exceeds maximum allowed limit of {MaxInputSizeBytes} bytes.", nameof(input));
        }

        // Extract candidate islands using regex
        var matches = ExpenseIslandRegex.Matches(input);
        var validatedIslands = new List<string>();

        foreach (Match match in matches)
        {
            var island = match.Value;

            // Validate XML structure with security hardening
            ValidateXmlIsland(island);

            validatedIslands.Add(island);
        }

        return validatedIslands;
    }

    private void ValidateXmlIsland(string xmlIsland)
    {
        // Check for DTD declarations before parsing (additional security layer)
        if (xmlIsland.Contains("<!DOCTYPE") || xmlIsland.Contains("<!ENTITY"))
        {
            throw new XmlException("DTD declarations are prohibited for security reasons.");
        }

        // Security-hardened XML reader settings
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,    // Block DTD declarations (security)
            XmlResolver = null,                         // Disable external entity resolution (XXE protection)
            MaxCharactersInDocument = 1_000_000         // DoS protection
        };

        using var stringReader = new StringReader(xmlIsland);
        using var xmlReader = XmlReader.Create(stringReader, settings);

        // Parse to validate structure (throws XmlException if malformed or contains DTD/XXE)
        XDocument.Load(xmlReader);
    }
}

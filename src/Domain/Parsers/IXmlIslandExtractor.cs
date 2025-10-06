namespace Flowingly.ParsingService.Domain.Parsers;

/// <summary>
/// Interface for extracting XML islands (expense blocks) from free-form text.
/// Implements secure XML parsing with DTD/XXE protection.
/// </summary>
public interface IXmlIslandExtractor
{
    /// <summary>
    /// Extracts all &lt;expense&gt;...&lt;/expense&gt; XML islands from input text.
    /// </summary>
    /// <param name="input">Free-form text containing XML islands</param>
    /// <returns>Collection of validated XML island strings</returns>
    /// <exception cref="System.Xml.XmlException">Thrown if XML is malformed or contains DTD/XXE attacks</exception>
    IEnumerable<string> Extract(string input);
}

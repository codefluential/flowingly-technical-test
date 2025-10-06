using FluentAssertions;
using Flowingly.ParsingService.Domain.Parsers;
using System.Xml;

namespace Flowingly.ParsingService.UnitTests.Parsers;

/// <summary>
/// TDD RED phase: Tests for secure XML island extraction
/// SECURITY NOTE: Tests XXE/DTD attack prevention and DoS protection
/// All tests MUST fail until IXmlIslandExtractor is implemented in task_026
/// </summary>
public class XmlIslandExtractorTests
{
    // Test fixture will instantiate once implementation exists
    // For now, these will fail with "type not found" errors

    [Fact]
    public void Should_Extract_Single_Expense_Island_From_Text()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = "Hi there, <expense><total>100</total><cost_centre>DEV</cost_centre></expense> thanks!";

        // Act
        var islands = extractor.Extract(input);

        // Assert
        islands.Should().NotBeNull();
        islands.Should().HaveCount(1);
        islands.First().Should().Contain("<total>100</total>");
        islands.First().Should().Contain("<cost_centre>DEV</cost_centre>");
        islands.First().Should().StartWith("<expense>");
        islands.First().Should().EndWith("</expense>");
    }

    [Fact]
    public void Should_Extract_Multiple_Expense_Islands_From_Same_Text()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = "First: <expense><total>100</total></expense> and second: <expense><total>200</total></expense>";

        // Act
        var islands = extractor.Extract(input);

        // Assert
        islands.Should().NotBeNull();
        islands.Should().HaveCount(2);
        islands.ElementAt(0).Should().Contain("<total>100</total>");
        islands.ElementAt(1).Should().Contain("<total>200</total>");
    }

    [Fact]
    public void Should_Return_Empty_When_No_Expense_Islands_Present()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = "Just some text with <vendor>Mojo</vendor> but no expense island";

        // Act
        var islands = extractor.Extract(input);

        // Assert
        islands.Should().NotBeNull();
        islands.Should().BeEmpty();
    }

    [Fact]
    public void Should_Extract_Expense_Island_With_Nested_Tags()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = "Order: <expense><items><item>Coffee</item><item>Lunch</item></items><total>50</total></expense>";

        // Act
        var islands = extractor.Extract(input);

        // Assert
        islands.Should().HaveCount(1);
        islands.First().Should().Contain("<items>");
        islands.First().Should().Contain("<item>Coffee</item>");
        islands.First().Should().Contain("<item>Lunch</item>");
        islands.First().Should().Contain("</items>");
        islands.First().Should().Contain("<total>50</total>");
    }

    [Fact]
    public void Should_Throw_XmlException_For_Malformed_XML_In_Island()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = "<expense><total>100</total><cost_centre>DEV</expense>"; // Missing </cost_centre>

        // Act
        Action act = () => extractor.Extract(input);

        // Assert - Malformed XML should be rejected
        act.Should().Throw<XmlException>()
            .WithMessage("*cost_centre*");
    }

    [Fact]
    public void Should_Extract_Empty_Expense_Island()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = "<expense></expense>";

        // Act
        var islands = extractor.Extract(input);

        // Assert - Empty island is extractable (validation happens later)
        islands.Should().HaveCount(1);
        islands.First().Should().Be("<expense></expense>");
    }

    [Fact]
    public void Should_Extract_Expense_Island_With_Attributes()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = "<expense type=\"reimbursement\" currency=\"NZD\"><total>100</total></expense>";

        // Act
        var islands = extractor.Extract(input);

        // Assert - Attributes should be preserved
        islands.Should().HaveCount(1);
        islands.First().Should().Contain("type=\"reimbursement\"");
        islands.First().Should().Contain("currency=\"NZD\"");
        islands.First().Should().Contain("<total>100</total>");
    }

    [Fact]
    public void Should_Prevent_XXE_Attack_With_External_Entity()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        // XXE attack: tries to read /etc/passwd via external entity
        var xxeAttack = "<!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><expense><total>&xxe;</total></expense>";

        // Act
        Action act = () => extractor.Extract(xxeAttack);

        // Assert - DTD processing should be blocked (SECURITY TEST)
        act.Should().Throw<XmlException>()
            .Where(ex => ex.Message.Contains("DTD") || ex.Message.Contains("prohibited") || ex.Message.Contains("entity"));
    }

    [Fact]
    public void Should_Reject_DTD_Declaration_In_Input()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        // DTD attack: external DTD reference
        var dtdAttack = "<!DOCTYPE expense SYSTEM \"http://malicious.com/evil.dtd\"><expense><total>100</total></expense>";

        // Act
        Action act = () => extractor.Extract(dtdAttack);

        // Assert - DtdProcessing = Prohibit should block this (SECURITY TEST)
        act.Should().Throw<XmlException>()
            .Where(ex => ex.Message.Contains("DTD") || ex.Message.Contains("prohibited"));
    }

    [Fact]
    public void Should_Enforce_Size_Limit_For_Large_XML_Islands()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        // DoS attack: very large XML (1MB+ of repeated content)
        var largeContent = new string('X', 2_000_000); // 2MB of 'X' characters
        var largeXmlAttack = $"<expense><data>{largeContent}</data></expense>";

        // Act
        Action act = () => extractor.Extract(largeXmlAttack);

        // Assert - Should reject or bound the input (DoS PREVENTION TEST)
        // Implementation should either:
        // 1. Throw exception for exceeding MaxCharactersInDocument
        // 2. Throw ArgumentException for input size validation
        // 3. Throw XmlException during parsing
        act.Should().Throw<Exception>()
            .Where(ex =>
                ex is XmlException ||
                ex is ArgumentException ||
                ex.Message.Contains("size") ||
                ex.Message.Contains("limit") ||
                ex.Message.Contains("too large"));
    }

    [Fact]
    public void Should_Handle_Mixed_Content_With_Non_Expense_XML()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = @"
            <vendor>Mojo Coffee</vendor>
            <expense><total>25</total></expense>
            <invoice>INV-123</invoice>
            <expense><total>50</total></expense>
        ";

        // Act
        var islands = extractor.Extract(input);

        // Assert - Should extract only <expense> islands, ignore others
        islands.Should().HaveCount(2);
        islands.Should().AllSatisfy(island =>
        {
            island.Should().Contain("<expense>");
            island.Should().NotContain("<vendor>");
            island.Should().NotContain("<invoice>");
        });
    }

    [Fact]
    public void Should_Extract_Expense_Island_With_CDATA_Section()
    {
        // Arrange
        var extractor = new XmlIslandExtractor();
        var input = "<expense><description><![CDATA[Special <chars> & symbols]]></description><total>100</total></expense>";

        // Act
        var islands = extractor.Extract(input);

        // Assert - CDATA sections should be preserved
        islands.Should().HaveCount(1);
        islands.First().Should().Contain("<![CDATA[");
        islands.First().Should().Contain("]]>");
        islands.First().Should().Contain("<total>100</total>");
    }
}

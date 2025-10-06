using FluentAssertions;
using Flowingly.ParsingService.Domain.Parsing;
using Xunit;

namespace Flowingly.ParsingService.UnitTests.Parsers;

/// <summary>
/// TDD RED phase: Comprehensive tests for ITimeParser interface (not yet implemented).
/// These tests MUST fail initially to demonstrate proper TDD methodology.
///
/// Business rules:
/// - Whitelist-based parsing (explicit formats only)
/// - Accepted: HH:mm, HH:mm:ss, h:mm tt, h:mm:ss tt
/// - Rejected: ambiguous formats (230, 2.30, 7.30pm)
/// - Ambiguous inputs return null (with warning logged)
/// </summary>
public class TimeParserTests
{
    // ITimeParser interface and TimeParser class don't exist yet - tests will fail
    private readonly ITimeParser _parser;

    public TimeParserTests()
    {
        // This will fail until task_024 implements TimeParser
        _parser = new TimeParser();
    }

    #region Valid 24-Hour Formats

    [Fact]
    public void Parse_TwentyFourHourFormat_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var input = "14:30";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().Be(new TimeSpan(14, 30, 0));
    }

    [Fact]
    public void Parse_TwentyFourHourFormatWithSeconds_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var input = "14:30:00";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().Be(new TimeSpan(14, 30, 0));
    }

    [Fact]
    public void Parse_MidnightTwentyFourHour_ReturnsZeroTimeSpan()
    {
        // Arrange
        var input = "00:00";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().Be(new TimeSpan(0, 0, 0));
    }

    #endregion

    #region Valid 12-Hour Formats

    [Fact]
    public void Parse_TwelveHourAMFormat_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var input = "7:30 AM";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().Be(new TimeSpan(7, 30, 0));
    }

    [Fact]
    public void Parse_TwelveHourPMFormat_ConvertsToTwentyFourHour()
    {
        // Arrange
        var input = "2:30 PM";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().Be(new TimeSpan(14, 30, 0));
    }

    [Fact]
    public void Parse_LeadingZeroTwelveHourAM_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var input = "07:30 AM";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().Be(new TimeSpan(7, 30, 0));
    }

    [Fact]
    public void Parse_MidnightTwelveHour_ReturnsZeroTimeSpan()
    {
        // Arrange
        var input = "12:00 AM";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().Be(new TimeSpan(0, 0, 0));
    }

    #endregion

    #region Ambiguous Formats (Must Reject)

    [Fact]
    public void Parse_AmbiguousNoSeparator_ReturnsNull()
    {
        // Arrange
        var input = "230"; // Could be 2:30 or 23:0 - ambiguous

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().BeNull("ambiguous format without separator must be rejected");
    }

    [Fact]
    public void Parse_AmbiguousDotSeparator_ReturnsNull()
    {
        // Arrange
        var input = "2.30"; // Dot separator is ambiguous

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().BeNull("dot separator is ambiguous and must be rejected");
    }

    [Fact]
    public void Parse_AmbiguousDotSeparatorWithPM_ReturnsNull()
    {
        // Arrange
        var input = "7.30pm"; // Dot separator with PM is unclear

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().BeNull("dot separator with PM is ambiguous and must be rejected");
    }

    #endregion

    #region Invalid and Empty Inputs

    [Fact]
    public void Parse_EmptyInput_ReturnsNull()
    {
        // Arrange
        var input = "";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().BeNull("empty input has no time to parse");
    }

    [Fact]
    public void Parse_InvalidTimeValues_ReturnsNull()
    {
        // Arrange
        var input = "25:99"; // Invalid: hour > 23, minute > 59

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().BeNull("invalid time values must be rejected");
    }

    #endregion

    #region Additional Edge Cases

    [Fact]
    public void Parse_NullInput_ReturnsNull()
    {
        // Arrange
        string? input = null;

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().BeNull("null input should return null");
    }

    [Fact]
    public void Parse_WhitespaceInput_ReturnsNull()
    {
        // Arrange
        var input = "   ";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().BeNull("whitespace-only input should return null");
    }

    #endregion
}

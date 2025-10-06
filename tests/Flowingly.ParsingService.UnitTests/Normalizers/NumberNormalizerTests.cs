using FluentAssertions;
using Flowingly.ParsingService.Domain.Normalizers;

namespace Flowingly.ParsingService.UnitTests.Normalizers;

/// <summary>
/// TDD RED Phase: Tests for INumberNormalizer interface
/// These tests MUST fail initially as the interface and implementation don't exist yet.
/// Tests cover currency symbol removal, comma stripping, and decimal parsing.
/// Reference: ADR-0008 (Parsing & Validation Rules), task_017
/// </summary>
public class NumberNormalizerTests
{
    /// <summary>
    /// Test data-driven scenarios for number normalization
    /// Using Theory/InlineData for comprehensive coverage
    /// </summary>
    [Theory]
    [InlineData("$35000.00", 35000.00)]
    [InlineData("£1234.56", 1234.56)]
    [InlineData("€999.99", 999.99)]
    [InlineData("35,000.00", 35000.00)]
    [InlineData("$35,000.00", 35000.00)]
    [InlineData("NZD 1,234.56", 1234.56)]
    [InlineData("1234.56", 1234.56)]
    [InlineData("1,234,567.89", 1234567.89)]
    [InlineData("$0.00", 0.00)]
    [InlineData("$0.99", 0.99)]
    public void Normalize_ShouldStripCurrencySymbolsAndCommas_WhenGivenFormattedNumbers(string input, decimal expected)
    {
        // Arrange
        var normalizer = new NumberNormalizer();

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Normalize_ShouldStripDollarSign_WhenGivenDollarPrefixedNumber()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "$35000.00";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(35000.00m);
    }

    [Fact]
    public void Normalize_ShouldStripPoundSign_WhenGivenPoundPrefixedNumber()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "£1234.56";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void Normalize_ShouldStripEuroSign_WhenGivenEuroPrefixedNumber()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "€999.99";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(999.99m);
    }

    [Fact]
    public void Normalize_ShouldRemoveCommas_WhenGivenNumberWithThousandsSeparators()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "35,000.00";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(35000.00m);
    }

    [Fact]
    public void Normalize_ShouldRemoveCurrencyAndCommas_WhenGivenMixedFormat()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "$35,000.00";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(35000.00m);
    }

    [Fact]
    public void Normalize_ShouldStripCurrencyCodePrefix_WhenGivenNZDPrefix()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "NZD 1,234.56";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void Normalize_ShouldParseDirectly_WhenGivenPlainNumber()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "1234.56";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void Normalize_ShouldRemoveMultipleCommas_WhenGivenLargeNumber()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "1,234,567.89";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(1234567.89m);
    }

    [Fact]
    public void Normalize_ShouldHandleZeroValue_WhenGivenFormattedZero()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "$0.00";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(0.00m);
    }

    [Fact]
    public void Normalize_ShouldPreserveSmallDecimals_WhenGivenSmallValue()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "$0.99";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(0.99m);
    }

    [Fact]
    public void Normalize_ShouldReturnDecimalType_NotDoubleOrFloat()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "$1234.56";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().BeOfType(typeof(decimal));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Normalize_ShouldThrowOrReturnDefault_WhenGivenInvalidInput(string input)
    {
        // Arrange
        var normalizer = new NumberNormalizer();

        // Act & Assert
        // This will fail in RED phase - implementation will determine behavior
        var act = () => normalizer.Normalize(input);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*invalid*", because: "empty or null input should be rejected");
    }

    [Fact]
    public void Normalize_ShouldHandleWhitespace_WhenGivenPaddedInput()
    {
        // Arrange
        var normalizer = new NumberNormalizer();
        var input = "  $1,234.56  ";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        result.Should().Be(1234.56m);
    }
}

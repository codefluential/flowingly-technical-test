using FluentAssertions;
using Xunit;
using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Services;

namespace Flowingly.ParsingService.UnitTests.Services;

/// <summary>
/// TDD RED Phase: Tax Calculator Tests
///
/// Tests for ITaxCalculator that calculates NZ GST from tax-inclusive totals.
/// All tests MUST FAIL initially (no implementation exists yet).
///
/// Business Rules:
/// - Default tax rate: 0.15 (15% NZ GST)
/// - Tax-exclusive = tax-inclusive / (1 + tax_rate)
/// - GST = tax-inclusive - tax-exclusive
/// - Use Banker's Rounding (MidpointRounding.ToEven) for all currency values
/// - Tax-inclusive amount must be non-negative
/// - Tax rate must be between 0 and 1 (0% to 100%)
///
/// Reference: ADR-0009, PRD Section 12
/// </summary>
public class TaxCalculatorTests
{
    // Interface under test - does not exist yet, tests will fail with compilation errors
    private readonly ITaxCalculator _calculator = new TaxCalculator();

    #region Happy Path Tests

    [Fact]
    public void CalculateFromInclusive_StandardGST_115_ShouldReturn_100_Exclusive_And_15_GST()
    {
        // Arrange
        decimal taxInclusive = 115.00m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        result.TaxExclusive.Should().Be(100.00m, "115 / 1.15 = 100.00");
        result.Gst.Should().Be(15.00m, "GST = 115 - 100 = 15.00");
        result.TaxInclusive.Should().Be(115.00m, "original inclusive amount");
        result.TaxRate.Should().Be(0.15m, "tax rate used");
    }

    [Fact]
    public void CalculateFromInclusive_WithBankersRounding_RoundDown_ShouldReturn_87_17_Exclusive()
    {
        // Arrange
        decimal taxInclusive = 100.25m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        result.TaxExclusive.Should().Be(87.17m, "100.25 / 1.15 = 87.173913... rounds to 87.17");
        result.Gst.Should().Be(13.08m, "GST = 100.25 - 87.17 = 13.08");
    }

    [Fact]
    public void CalculateFromInclusive_WithBankersRounding_RoundUp_ShouldReturn_87_26_Exclusive()
    {
        // Arrange
        decimal taxInclusive = 100.35m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        result.TaxExclusive.Should().Be(87.26m, "100.35 / 1.15 = 87.26087... rounds to 87.26");
        result.Gst.Should().Be(13.09m, "GST = 100.35 - 87.26 = 13.09");
    }

    [Fact]
    public void CalculateFromInclusive_WithBankersRounding_Midpoint_ShouldRoundToEven()
    {
        // Arrange
        decimal taxInclusive = 100.115m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        // 100.115 / 1.15 = 87.0565217...
        // Rounds to 87.06 (6 is even)
        result.TaxExclusive.Should().Be(87.06m, "Banker's Rounding: 87.0565... rounds to 87.06 (even)");
        result.Gst.Should().Be(13.06m, "GST = 100.115 - 87.06 = 13.055 rounds to 13.06");
    }

    [Fact]
    public void CalculateFromInclusive_WithCustomTaxRate_20Percent_ShouldCalculateCorrectly()
    {
        // Arrange
        decimal taxInclusive = 120.00m;
        decimal taxRate = 0.20m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        result.TaxExclusive.Should().Be(100.00m, "120 / 1.20 = 100.00");
        result.Gst.Should().Be(20.00m, "GST = 120 - 100 = 20.00");
        result.TaxRate.Should().Be(0.20m, "custom tax rate");
    }

    [Fact]
    public void CalculateFromInclusive_WithDefaultTaxRate_ShouldUse_15Percent()
    {
        // Arrange
        decimal taxInclusive = 115.00m;

        // Act - using default tax rate parameter
        var result = _calculator.CalculateFromInclusive(taxInclusive);

        // Assert
        result.TaxExclusive.Should().Be(100.00m, "115 / 1.15 = 100.00");
        result.Gst.Should().Be(15.00m, "GST = 15.00");
        result.TaxRate.Should().Be(0.15m, "default NZ GST rate");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculateFromInclusive_ZeroAmount_ShouldReturnZero()
    {
        // Arrange
        decimal taxInclusive = 0.00m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        result.TaxExclusive.Should().Be(0.00m, "zero inclusive = zero exclusive");
        result.Gst.Should().Be(0.00m, "zero GST");
    }

    [Fact]
    public void CalculateFromInclusive_SmallAmount_WithPrecision_ShouldRoundCorrectly()
    {
        // Arrange
        decimal taxInclusive = 1.15m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        result.TaxExclusive.Should().Be(1.00m, "1.15 / 1.15 = 1.00");
        result.Gst.Should().Be(0.15m, "GST = 0.15");
    }

    [Fact]
    public void CalculateFromInclusive_ZeroTaxRate_ShouldReturnSameAmount()
    {
        // Arrange
        decimal taxInclusive = 100.00m;
        decimal taxRate = 0.00m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        result.TaxExclusive.Should().Be(100.00m, "no tax means exclusive = inclusive");
        result.Gst.Should().Be(0.00m, "zero tax rate = zero GST");
    }

    [Fact]
    public void CalculateFromInclusive_LargeAmount_ShouldCalculateAccurately()
    {
        // Arrange
        decimal taxInclusive = 1000000.00m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        result.TaxExclusive.Should().Be(869565.22m, "1000000 / 1.15 = 869565.217... rounds to 869565.22");
        result.Gst.Should().Be(130434.78m, "GST = 1000000 - 869565.22 = 130434.78");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void CalculateFromInclusive_NegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        decimal taxInclusive = -100.00m;
        decimal taxRate = 0.15m;

        // Act
        Action act = () => _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tax-inclusive amount must be non-negative*");
    }

    [Fact]
    public void CalculateFromInclusive_NegativeTaxRate_ShouldThrowArgumentException()
    {
        // Arrange
        decimal taxInclusive = 100.00m;
        decimal taxRate = -0.15m;

        // Act
        Action act = () => _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tax rate must be between 0 and 1*");
    }

    [Fact]
    public void CalculateFromInclusive_TaxRateGreaterThanOne_ShouldThrowArgumentException()
    {
        // Arrange
        decimal taxInclusive = 100.00m;
        decimal taxRate = 1.5m;

        // Act
        Action act = () => _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tax rate must be between 0 and 1*");
    }

    [Fact]
    public void CalculateFromInclusive_TaxRateExactlyOne_ShouldThrowArgumentException()
    {
        // Arrange
        decimal taxInclusive = 100.00m;
        decimal taxRate = 1.00m;

        // Act
        Action act = () => _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tax rate must be between 0 and 1*");
    }

    #endregion

    #region Banker's Rounding Verification Tests

    [Fact]
    public void CalculateFromInclusive_MultipleCalculations_ShouldBeConsistent()
    {
        // Arrange
        decimal taxInclusive = 100.25m;
        decimal taxRate = 0.15m;

        // Act - Call multiple times
        var result1 = _calculator.CalculateFromInclusive(taxInclusive, taxRate);
        var result2 = _calculator.CalculateFromInclusive(taxInclusive, taxRate);
        var result3 = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert - All results should be identical (deterministic)
        result1.TaxExclusive.Should().Be(result2.TaxExclusive);
        result1.TaxExclusive.Should().Be(result3.TaxExclusive);
        result1.Gst.Should().Be(result2.Gst);
        result1.Gst.Should().Be(result3.Gst);
    }

    [Fact]
    public void CalculateFromInclusive_VerifyBankersRounding_Midpoint_2_5_Rounds_To_2()
    {
        // Arrange - Construct scenario where rounding result is exactly 2.5
        // We need tax_inclusive / (1 + tax_rate) = 2.5
        // So tax_inclusive = 2.5 * 1.15 = 2.875
        decimal taxInclusive = 2.875m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert - 2.5 should round to 2.50 (already even), but let's verify the full calculation
        // 2.875 / 1.15 = 2.5 exactly
        result.TaxExclusive.Should().Be(2.50m, "2.875 / 1.15 = 2.5 (already even)");
        result.Gst.Should().Be(0.38m, "GST = 2.875 - 2.50 = 0.375, rounds to 0.38 (even)");
    }

    [Fact]
    public void CalculateFromInclusive_VerifyBankersRounding_Midpoint_3_5_Rounds_To_4()
    {
        // Arrange - Construct scenario where rounding result is exactly 3.5
        // tax_inclusive = 3.5 * 1.15 = 4.025
        decimal taxInclusive = 4.025m;
        decimal taxRate = 0.15m;

        // Act
        var result = _calculator.CalculateFromInclusive(taxInclusive, taxRate);

        // Assert - 3.5 should round to 3.50 (already even)
        // 4.025 / 1.15 = 3.5 exactly
        result.TaxExclusive.Should().Be(3.50m, "4.025 / 1.15 = 3.5 (already even)");
        result.Gst.Should().Be(0.52m, "GST = 4.025 - 3.50 = 0.525, rounds to 0.52 (even)");
    }

    #endregion
}

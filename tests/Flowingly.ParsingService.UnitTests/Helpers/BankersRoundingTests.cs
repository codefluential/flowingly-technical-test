using FluentAssertions;
using Xunit;
using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Helpers;

namespace Flowingly.ParsingService.UnitTests.Helpers;

/// <summary>
/// TDD RED Phase: Banker's Rounding Tests
///
/// Tests for MidpointRounding.ToEven behavior - critical for GST calculation accuracy.
/// All tests MUST FAIL initially (no implementation exists yet).
///
/// Banker's Rounding: When value is exactly at midpoint (e.g., 2.125), round to nearest EVEN number.
/// - 2.125 → 2.12 (2 is even, round down)
/// - 2.135 → 2.14 (4 is even, round up)
///
/// Reference: ADR-0009, PRD Section 9.3
/// </summary>
public class BankersRoundingTests
{
    // Interface under test - does not exist yet, tests will fail with compilation errors
    private readonly IRoundingHelper _roundingHelper = new RoundingHelper(); // These types don't exist yet

    [Fact]
    public void Round_Midpoint_2_125_ShouldRoundDown_ToEven_2_12()
    {
        // Arrange
        decimal input = 2.125m;
        int precision = 2;
        decimal expected = 2.12m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "2 is even, so 2.125 rounds down to 2.12");
    }

    [Fact]
    public void Round_Midpoint_2_135_ShouldRoundUp_ToEven_2_14()
    {
        // Arrange
        decimal input = 2.135m;
        int precision = 2;
        decimal expected = 2.14m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "4 is even, so 2.135 rounds up to 2.14");
    }

    [Fact]
    public void Round_Midpoint_2_145_ShouldRoundDown_ToEven_2_14()
    {
        // Arrange
        decimal input = 2.145m;
        int precision = 2;
        decimal expected = 2.14m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "4 is even, so 2.145 rounds down to 2.14");
    }

    [Fact]
    public void Round_Midpoint_2_155_ShouldRoundUp_ToEven_2_16()
    {
        // Arrange
        decimal input = 2.155m;
        int precision = 2;
        decimal expected = 2.16m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "6 is even, so 2.155 rounds up to 2.16");
    }

    [Fact]
    public void Round_EdgeCase_0_005_ShouldRoundDown_ToEven_0_00()
    {
        // Arrange
        decimal input = 0.005m;
        int precision = 2;
        decimal expected = 0.00m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "0 is even, so 0.005 rounds down to 0.00");
    }

    [Fact]
    public void Round_EdgeCase_0_015_ShouldRoundUp_ToEven_0_02()
    {
        // Arrange
        decimal input = 0.015m;
        int precision = 2;
        decimal expected = 0.02m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "2 is even, so 0.015 rounds up to 0.02");
    }

    [Fact]
    public void Round_EdgeCase_0_025_ShouldRoundDown_ToEven_0_02()
    {
        // Arrange
        decimal input = 0.025m;
        int precision = 2;
        decimal expected = 0.02m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "2 is even, so 0.025 rounds down to 0.02");
    }

    [Fact]
    public void Round_NegativeMidpoint_Minus2_125_ShouldRoundDown_ToEven_Minus2_12()
    {
        // Arrange
        decimal input = -2.125m;
        int precision = 2;
        decimal expected = -2.12m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "Banker's rounding works with negative numbers: -2.125 → -2.12");
    }

    [Fact]
    public void Round_NonMidpoint_2_126_ShouldRoundNormally_To_2_13()
    {
        // Arrange
        decimal input = 2.126m;
        int precision = 2;
        decimal expected = 2.13m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "Not a midpoint value, standard rounding applies: 2.126 → 2.13");
    }

    [Fact]
    public void Round_AlreadyAtPrecision_2_12_ShouldRemainUnchanged()
    {
        // Arrange
        decimal input = 2.12m;
        int precision = 2;
        decimal expected = 2.12m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "Already at desired precision, no rounding needed");
    }

    [Fact]
    public void Round_LargeValue_1234_565_ShouldRoundUp_ToEven_1234_56()
    {
        // Arrange
        decimal input = 1234.565m;
        int precision = 2;
        decimal expected = 1234.56m;

        // Act
        var result = _roundingHelper.Round(input, precision);

        // Assert
        result.Should().Be(expected, "6 is even, so 1234.565 rounds down to 1234.56");
    }

    [Fact]
    public void Round_GSTScenario_TotalInclusive_115_00_ShouldCalculateCorrectExclusive()
    {
        // Arrange - Real GST calculation scenario
        decimal totalInclusive = 115.00m;
        decimal gstRate = 0.15m;

        // Act - Calculate exclusive total with Banker's Rounding
        decimal totalExclusive = totalInclusive / (1 + gstRate);
        totalExclusive = _roundingHelper.Round(totalExclusive, 2);
        decimal salesTax = _roundingHelper.Round(totalInclusive - totalExclusive, 2);

        // Assert
        totalExclusive.Should().Be(100.00m, "115 / 1.15 = 100.00 (no rounding needed)");
        salesTax.Should().Be(15.00m, "GST should be exactly 15.00");
    }
}

# ADR-0009: Tax Calculation with Banker's Rounding

**Status**: Accepted
**Date**: 2025-10-06
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service processes expense data that includes monetary calculations, specifically:
- **Total (tax-inclusive)**: The total amount paid, including GST
- **Total (tax-exclusive)**: The base amount before tax
- **Sales Tax**: The GST amount

These calculations require **rounding to 2 decimal places** (standard for currency). However, different rounding strategies introduce different biases:

### Rounding Strategies

1. **Standard Rounding (0.5 always rounds up)**:
   - `2.125 → 2.13`
   - `2.135 → 2.14`
   - `2.145 → 2.15`
   - **Bias**: Upward (always rounds 0.5 up)

2. **Banker's Rounding (0.5 rounds to nearest even)**:
   - `2.125 → 2.12` (round to even)
   - `2.135 → 2.14` (round to even)
   - `2.145 → 2.14` (round to even)
   - `2.155 → 2.16` (round to even)
   - **Bias**: None (statistically unbiased over many operations)

3. **Truncation (always round down)**:
   - `2.125 → 2.12`
   - `2.135 → 2.13`
   - **Bias**: Downward

The PRD v0.3 (Section 4.2) specifies **Banker's Rounding** (MidpointRounding.ToEven) as the policy for all monetary calculations.

### Tax Calculation Formula

Given:
- `total_incl_tax` (tax-inclusive total, e.g., `$120.50`)
- `tax_rate` (e.g., `0.15` for 15% GST)

Calculate:
```
total_excl_tax = total_incl_tax / (1 + tax_rate)
sales_tax = total_incl_tax - total_excl_tax
```

Round both `total_excl_tax` and `sales_tax` to 2 decimal places using Banker's Rounding.

**Example**:
```
total_incl_tax = 120.50
tax_rate = 0.15

total_excl_tax = 120.50 / 1.15 = 104.78260869565217...
                 → 104.78 (Banker's Rounding)

sales_tax = 120.50 - 104.78 = 15.72
```

## Decision

**Use Banker's Rounding (MidpointRounding.ToEven) for all monetary values.**

Specifically:
- Apply Banker's Rounding at **business boundaries** (after tax calculation, before persistence/response)
- Use `Math.Round(value, 2, MidpointRounding.ToEven)` in C#
- Round `total_excl_tax` and `sales_tax` independently (do NOT force them to sum exactly to `total_incl_tax`)

### Banker's Rounding Policy

**Midpoint Rule**: When rounding `X.XX5`, round to the nearest **even** digit.

**Examples**:
- `2.125 → 2.12` (2 is even)
- `2.135 → 2.14` (4 is even)
- `2.145 → 2.14` (4 is even)
- `2.155 → 2.16` (6 is even)
- `2.165 → 2.16` (6 is even)
- `2.175 → 2.18` (8 is even)

**Non-Midpoint**: Standard rounding applies.
- `2.123 → 2.12`
- `2.127 → 2.13`

## Consequences

### Positive

1. **Unbiased Rounding**: Statistically neutral over many operations (no cumulative upward/downward bias)
2. **Financial Standards**: Matches IEEE 754 standard and common financial practices
3. **Statistical Accuracy**: Better for aggregations (summing many rounded values)
4. **Predictable**: Deterministic behavior (same input → same output)
5. **C# Native Support**: `MidpointRounding.ToEven` is built into .NET

### Negative

1. **Less Intuitive**: Users expect 0.5 to always round up (traditional school rounding)
2. **Requires Explanation**: Documentation must clarify rounding policy
3. **Edge Case Surprises**: `2.125 → 2.12` may confuse users expecting `2.13`

### Mitigation

- **Documentation**: Clearly document Banker's Rounding policy in README and API docs
- **Test Coverage**: Unit tests for all edge cases (see below)
- **Transparency**: Include `taxRate` in response so users can verify calculations

## Alternatives Considered

### Alternative 1: Standard Rounding (0.5 always rounds up)

**Approach**: Use `Math.Round(value, 2, MidpointRounding.AwayFromZero)`

**Pros**:
- More intuitive (matches traditional school rounding)
- Easier to explain

**Cons**:
- **Upward bias**: Over many operations, totals skew higher
- Not statistically neutral
- Can accumulate errors in aggregations

**Rejected because**: Upward bias is undesirable for financial calculations; Banker's Rounding is more accurate.

### Alternative 2: Truncation (always round down)

**Approach**: Use `Math.Floor(value * 100) / 100`

**Pros**:
- Simple implementation
- Conservative (never overstates amounts)

**Cons**:
- **Downward bias**: Always rounds down, underestimates totals
- Not fair to users (they pay slightly less tax, but also record slightly less expense)
- Not standard practice

**Rejected because**: Downward bias is as undesirable as upward bias; not statistically neutral.

### Alternative 3: No Rounding (Exact Decimal)

**Approach**: Use `decimal` type without rounding, store exact values.

**Pros**:
- No precision loss
- Exact calculations

**Cons**:
- **Impractical for currency display**: `$104.78260869565217` is not user-friendly
- Database storage issues (requires high-precision decimal columns)
- JSON serialization complexity (arbitrary precision)

**Rejected because**: Currency must be displayed in 2 decimal places; rounding is necessary.

### Alternative 4: Force Sales Tax + Total Excl Tax = Total Incl Tax

**Approach**: Round `total_excl_tax`, then compute `sales_tax = total_incl_tax - total_excl_tax` (no rounding).

**Example**:
```
total_incl_tax = 120.50
total_excl_tax = round(120.50 / 1.15) = 104.78
sales_tax = 120.50 - 104.78 = 15.72 (exact)
```

**Pros**:
- Guaranteed to sum correctly (no rounding discrepancies)

**Cons**:
- `sales_tax` may not be properly rounded (could have 3+ decimal places in edge cases)
- Inconsistent rounding (one field rounded, another not)

**Rejected because**: All monetary fields should be consistently rounded; minor discrepancies (e.g., 0.01 difference) are acceptable and standard in financial systems.

## Implementation

### Tax Calculator with Banker's Rounding

```csharp
public class TaxCalculator : ITaxCalculator
{
    public TaxBreakdown Calculate(decimal totalInclTax, decimal taxRate)
    {
        if (totalInclTax < 0)
            throw new ArgumentException("Total must be non-negative", nameof(totalInclTax));

        if (taxRate < 0 || taxRate > 1)
            throw new ArgumentException("Tax rate must be between 0 and 1", nameof(taxRate));

        // Calculate exclusive total
        var totalExclTax = totalInclTax / (1 + taxRate);
        totalExclTax = Math.Round(totalExclTax, 2, MidpointRounding.ToEven);

        // Calculate sales tax
        var salesTax = totalInclTax - totalExclTax;
        salesTax = Math.Round(salesTax, 2, MidpointRounding.ToEven);

        return new TaxBreakdown
        {
            TotalInclTax = totalInclTax,
            TotalExclTax = totalExclTax,
            SalesTax = salesTax,
            TaxRate = taxRate
        };
    }
}

public class TaxBreakdown
{
    public decimal TotalInclTax { get; set; }
    public decimal TotalExclTax { get; set; }
    public decimal SalesTax { get; set; }
    public decimal TaxRate { get; set; }
}
```

### Edge Case Handling

**Rounding Discrepancies**:

Due to independent rounding, `total_excl_tax + sales_tax` may not exactly equal `total_incl_tax` (off by 0.01 in rare cases).

**Example**:
```
total_incl_tax = 100.00
tax_rate = 0.15

total_excl_tax = 100 / 1.15 = 86.95652173913...
                 → 86.96 (Banker's Rounding)

sales_tax = 100 - 86.96 = 13.04
            → 13.04 (Banker's Rounding)

Verification: 86.96 + 13.04 = 100.00 ✅
```

**Another Example**:
```
total_incl_tax = 120.50
tax_rate = 0.15

total_excl_tax = 120.50 / 1.15 = 104.78260869565...
                 → 104.78

sales_tax = 120.50 - 104.78 = 15.72
            → 15.72

Verification: 104.78 + 15.72 = 120.50 ✅
```

In practice, discrepancies are extremely rare and acceptable in financial systems (standard practice).

### Business Boundary Application

**When to Round**:
- **After tax calculation**: Before persisting to database
- **Before response**: Before returning to API client

**When NOT to Round**:
- During intermediate calculations (preserve precision)
- When aggregating values (sum first, then round final result)

**Example (Aggregation)**:
```csharp
// ❌ BAD: Round each value, then sum (cumulative rounding errors)
var total = expenses.Sum(e => Math.Round(e.Total, 2));

// ✅ GOOD: Sum first, then round final result
var total = Math.Round(expenses.Sum(e => e.Total), 2, MidpointRounding.ToEven);
```

## Testing Strategy

### Unit Tests for Banker's Rounding Edge Cases

```csharp
[Theory]
[InlineData(2.125, 2.12)]  // Round to even (2)
[InlineData(2.135, 2.14)]  // Round to even (4)
[InlineData(2.145, 2.14)]  // Round to even (4)
[InlineData(2.155, 2.16)]  // Round to even (6)
[InlineData(2.165, 2.16)]  // Round to even (6)
[InlineData(2.175, 2.18)]  // Round to even (8)
public void BankersRounding_MidpointCases(decimal input, decimal expected)
{
    var result = Math.Round(input, 2, MidpointRounding.ToEven);
    result.Should().Be(expected);
}

[Theory]
[InlineData(120.50, 0.15, 104.78, 15.72)]  // Standard case
[InlineData(100.00, 0.15, 86.96, 13.04)]   // Even total
[InlineData(99.99, 0.15, 86.95, 13.04)]    // Edge case
[InlineData(50.00, 0.15, 43.48, 6.52)]     // Small amount
public void TaxCalculation_WithBankersRounding(
    decimal totalInclTax,
    decimal taxRate,
    decimal expectedExclTax,
    decimal expectedSalesTax)
{
    var calculator = new TaxCalculator();
    var result = calculator.Calculate(totalInclTax, taxRate);

    result.TotalExclTax.Should().Be(expectedExclTax);
    result.SalesTax.Should().Be(expectedSalesTax);
}
```

### BDD Scenario (see ADR-0010 for full test plan)

**Scenario: Tax calculation uses Banker's Rounding**

```
GIVEN an expense with total_incl_tax = 120.50 and tax_rate = 0.15
WHEN calculating tax breakdown
THEN total_excl_tax = 104.78 (Banker's Rounding)
  AND sales_tax = 15.72
  AND 104.78 + 15.72 = 120.50 (verification)
```

## References

- PRD + Technical Specification v0.3, Section 4.2: Parsing Rules (Banker's Rounding policy)
- PRD v0.3, Section 6: Document History (rounding policy specification)
- ADR-0010: Test Strategy and Coverage (Banker's Rounding test scenarios)
- IEEE 754 Floating Point Standard: https://en.wikipedia.org/wiki/IEEE_754#Rounding_rules
- C# MidpointRounding Enum: https://learn.microsoft.com/en-us/dotnet/api/system.midpointrounding
- Banker's Rounding Explained: https://en.wikipedia.org/wiki/Rounding#Round_half_to_even

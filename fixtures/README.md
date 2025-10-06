# Test Fixtures

This directory contains sample email content extracted from the Flowingly Technical Test brief for use in unit, integration, and E2E tests.

## Purpose

These fixtures provide realistic test data for validating:
- Text parsing and tag extraction
- Content classification (expense vs other/unprocessed)
- Tag validation (balanced opening/closing tags)
- GST tax calculation from tax-inclusive totals
- Error handling for missing required fields

## Fixture Files

### sample-email-1-expense.txt

**Classification**: Expense
**Source**: Test brief PDF page 1 (first email example)
**Contains**: XML island with expense data

**Key Features**:
- XML island: `<expense>...</expense>` containing structured data
- Required fields: `<cost_centre>`, `<total>`, `<payment_method>`
- Tax-inclusive total: 35,000 (NZD implied, 15% GST rate)
- Valid for testing GST calculation: total_exclusive = 35,000 / 1.15 = 30,434.78, tax = 4,565.22

**Expected Parsing Behavior**:
- Classification: "expense"
- Extracted fields: cost_centre=DEV632, total=35000, payment_method=personal card
- Tax calculation required (tax-inclusive to exclusive conversion)

### sample-email-2-other.txt

**Classification**: Other/Unprocessed
**Source**: Test brief PDF page 1 (second email example)
**Contains**: Inline tags for reservation (non-expense content)

**Key Features**:
- Inline tags: `<vendor>`, `<description>`, `<date>` (not expense-related)
- NO `<total>` tag (not an expense)
- NO `<cost_centre>` tag
- Valid for testing content routing to "other" classification

**Expected Parsing Behavior**:
- Classification: "other"
- Extracted fields: vendor=Seaside Steakhouse, description=development team's project end celebration, date=27 April 2022
- NO tax calculation (not an expense)

## Usage in Tests

### C# (xUnit)

```csharp
// Read fixture content
var expenseContent = File.ReadAllText("fixtures/sample-email-1-expense.txt");
var otherContent = File.ReadAllText("fixtures/sample-email-2-other.txt");

// Use in test
[Fact]
public async Task Should_Parse_Expense_Email()
{
    var fixture = File.ReadAllText("fixtures/sample-email-1-expense.txt");
    var result = await _parser.ParseAsync(fixture);

    Assert.Equal("expense", result.Classification);
    Assert.Equal("DEV632", result.CostCentre);
    Assert.Equal(35000m, result.Total);
}
```

### TypeScript (Playwright E2E)

```typescript
import { readFileSync } from 'fs';

const expenseFixture = readFileSync('./fixtures/sample-email-1-expense.txt', 'utf-8');
const otherFixture = readFileSync('./fixtures/sample-email-2-other.txt', 'utf-8');

test('should parse expense email from UI', async ({ page }) => {
  await page.goto('http://localhost:3000');
  await page.fill('textarea[name="emailContent"]', expenseFixture);
  await page.click('button:has-text("Submit")');

  // Verify parsed output
  const output = await page.textContent('.parsed-output');
  expect(output).toContain('DEV632');
});
```

## File Format

- **Encoding**: UTF-8
- **Format**: Plain text (.txt)
- **Line Endings**: Unix (LF)
- **Naming Convention**: `sample-email-{number}-{classification}.txt`

## Validation Rules

Based on the test brief requirements:

1. **Tag Integrity**: All opening tags must have corresponding closing tags
2. **Required Field (Expense)**: `<total>` is mandatory for expense classification; reject if missing
3. **Optional Field (Expense)**: `<cost_centre>` defaults to "UNKNOWN" if missing
4. **Tax Calculation**: Total is tax-inclusive (15% NZ GST); must calculate tax-exclusive amount and tax

## Adding New Fixtures

When adding new fixture files:
1. Extract content directly from test brief or create realistic examples
2. Follow naming convention: `sample-email-{number}-{classification}.txt`
3. Ensure UTF-8 encoding
4. Document in this README with classification and expected behavior
5. Include edge cases: missing tags, unclosed tags, boundary values

## Test Coverage

These fixtures support:
- ✅ Unit tests: Tag validation, XML parsing, tax calculation, normalization
- ✅ Integration tests: End-to-end parsing flow, content routing, database persistence
- ✅ E2E tests: UI interaction, API calls, error display, JSON output rendering

## Notes

- The test brief provides two sample emails (page 1); these are preserved exactly as shown
- Expense fixture uses comma as thousands separator (35,000) for testing number normalization
- Other fixture demonstrates reservation content (Phase 2 feature, stored as "other" in Phase 1)
- Both fixtures test different tag styles: XML islands vs inline tags

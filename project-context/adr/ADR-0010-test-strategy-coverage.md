# ADR-0010: Test Strategy and Coverage

**Status**: Accepted
**Date**: 2025-10-06
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service requires comprehensive test coverage to ensure:
- **Correctness**: Parsing and validation logic works as specified
- **Regression Prevention**: Changes don't break existing functionality
- **Confidence**: Safe deployment to production

The PRD v0.3 (Section 13) significantly expanded test requirements:
- **6 new BDD scenarios** added (overlapping tags, Banker's Rounding, tax rate precedence)
- **Testing matrix** defines unit/integration/E2E coverage boundaries
- **Sample email fixtures** from project brief required for E2E tests

### Testing Pyramid

```
        /\
       /E2E\        ← Few, high-value scenarios (critical paths)
      /------\
     /  Integ \     ← Moderate, API + DB tests (full pipeline)
    /----------\
   /    Unit    \   ← Many, fast tests (domain logic isolation)
  /--------------\
```

**Test Types**:
1. **Unit Tests**: Domain logic in isolation (parsers, calculators, validators, normalizers)
2. **Integration Tests**: API endpoints with in-memory DB (full request → response pipeline)
3. **E2E Tests**: Playwright against running app + DB (user workflows, critical paths)

## Decision

**Implement a three-tier testing strategy aligned with PRD v0.3 Section 13 matrix.**

### Tier 1: Unit Tests (Domain Logic)

**Scope**: Test domain logic in isolation without external dependencies.

**Components to Test**:
- `ITagValidator`: Stack-based tag validation (overlapping tags, unclosed tags)
- `ITaxCalculator`: Banker's Rounding, tax breakdown calculations
- `INormalizer`: Number/date/time normalization
- `IContentRouter`: Classification logic (expense vs other)
- Value Objects: Validation rules (e.g., `Money`, `TaxRate`)

**Technology**: xUnit + FluentAssertions

**Coverage Target**: 80%+ code coverage

**Examples**:
```csharp
// Tag Validator Tests
[Theory]
[InlineData("<a><b></b></a>", true)]            // Valid nesting
[InlineData("<a><b></a></b>", false)]           // Overlapping tags
[InlineData("<total>120</total>", true)]        // Valid single tag
[InlineData("<total>120", false)]               // Unclosed tag
public void TagValidator_ValidatesNesting(string input, bool expectedValid)
{
    var validator = new TagValidator();
    var result = validator.Validate(input);
    result.IsValid.Should().Be(expectedValid);
}

// Tax Calculator Tests (Banker's Rounding)
[Theory]
[InlineData(120.50, 0.15, 104.78, 15.72)]
[InlineData(100.00, 0.15, 86.96, 13.04)]
public void TaxCalculator_UsesBankersRounding(
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

### Tier 2: Integration Tests (API + DB)

**Scope**: Test full request → response pipeline with in-memory database.

**Components to Test**:
- API endpoints (`POST /api/v1/parse`)
- Full processing pipeline (validate → extract → normalize → persist → respond)
- Database persistence (EF Core with in-memory SQLite)
- Error handling (400 for validation errors, 500 for server errors)

**Technology**: xUnit + WebApplicationFactory (ASP.NET Core integration testing)

**Database**: In-memory SQLite (`Data Source=:memory:`)

**Coverage Target**: 60%+ coverage (happy path + critical error scenarios)

**Examples**:
```csharp
public class ParseEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ParseEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ParseEndpoint_ExpenseClassification_ReturnsExpenseResponse()
    {
        // Arrange
        var request = new
        {
            content = "Expense for <vendor>Mojo Coffee</vendor> total <total>120.50</total>",
            tax_rate = 0.15
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ParseResponse>();
        result.Classification.Should().Be("expense");
        result.Expense.Should().NotBeNull();
        result.Expense.Vendor.Should().Be("Mojo Coffee");
        result.Expense.Total.Should().Be(120.50m);
    }

    [Fact]
    public async Task ParseEndpoint_UnclosedTag_Returns400()
    {
        // Arrange
        var request = new { content = "<total>120" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Message.Should().Contain("Unclosed tag");
    }
}
```

### Tier 3: E2E Tests (Playwright)

**Scope**: Test critical user workflows against running application + database.

**Components to Test**:
- Full user workflows (submit text → view results)
- UI rendering (React components)
- API integration from UI
- Sample email fixtures from project brief

**Technology**: Playwright (TypeScript)

**Database**: Test instance of Postgres (or SQLite for speed)

**Coverage Target**: Happy path + critical error scenarios (not exhaustive)

**Sample Fixtures Required** (from PRD v0.3 Section 13):
- Sample Email 1 from brief (expense email example)
- Sample Email 2 from brief (other email example)

**Examples**:
```typescript
// E2E Test: Submit Sample Email 1
test('submit expense email returns structured response', async ({ page }) => {
  await page.goto('http://localhost:5173');

  // Load Sample Email 1 content
  const sampleEmail1 = `
    Hi everyone,
    Expense for <vendor>Mojo Coffee</vendor> total <total>120.50</total>
    <description>Team lunch</description> <cost_centre>DEV</cost_centre>
    Thanks!
  `;

  await page.fill('[data-testid="content-input"]', sampleEmail1);
  await page.click('[data-testid="submit-button"]');

  // Assert response
  await expect(page.locator('[data-testid="classification"]')).toHaveText('expense');
  await expect(page.locator('[data-testid="vendor"]')).toHaveText('Mojo Coffee');
  await expect(page.locator('[data-testid="total"]')).toHaveText('120.50');
});
```

## Consequences

### Positive

1. **Comprehensive Coverage**: Three-tier strategy covers domain logic, API contracts, and user workflows
2. **Clear Test Ownership**: Each tier has specific responsibilities (no overlap or gaps)
3. **Prevents Regressions**: Automated tests catch breaking changes before deployment
4. **Fast Feedback**: Unit tests run in seconds; integration tests in minutes; E2E tests on demand
5. **Confidence in Deployment**: High coverage ensures production readiness

### Negative

1. **More Test Code**: Test suite will be larger than application code (typical 2:1 ratio)
2. **Maintenance Overhead**: Tests must be updated when application logic changes
3. **Longer CI Runs**: Full test suite (unit + integration + E2E) may take 5-10 minutes

### Mitigation

- **Fast Tests First**: Run unit tests in CI first (fail fast)
- **Parallel Execution**: Run E2E tests in parallel to reduce wall-clock time
- **Selective E2E**: Only run E2E tests on main branch or pre-deployment (not every commit)

## New Testing Requirements from PRD v0.3

### BDD Scenarios Added in v0.3

1. **Overlapping Tag Validation**
   - GIVEN content with overlapping tags `<a><b></a></b>`
   - WHEN parsing
   - THEN validation fails with error "Overlapping tags: expected `</b>`, found `</a>`"

2. **Banker's Rounding Edge Cases**
   - GIVEN expense with total 120.50 and tax rate 0.15
   - WHEN calculating tax breakdown
   - THEN total_excl_tax = 104.78 (Banker's Rounding)
   - AND sales_tax = 15.72

3. **Tax Rate Precedence: Request Parameter Wins**
   - GIVEN request with tax_rate = 0.10 AND config default = 0.15
   - WHEN resolving tax rate
   - THEN result is 0.10

4. **Tax Rate Precedence: Config Default Fallback**
   - GIVEN request with no tax_rate AND config default = 0.15
   - WHEN resolving tax rate
   - THEN result is 0.15

5. **Tax Rate Precedence: Strict Mode Error**
   - GIVEN request with no tax_rate AND no config default AND StrictTaxRate = true
   - WHEN resolving tax rate
   - THEN throws ValidationException "Tax rate is required"

6. **Ambiguous Time Handling**
   - GIVEN content with time "230" (ambiguous)
   - WHEN parsing time
   - THEN result is null AND warning logged

### Testing Matrix (PRD v0.3 Section 13)

| Component | Unit Tests | Integration Tests | E2E Tests |
|-----------|------------|-------------------|-----------|
| Tag Validator | ✅ All edge cases | ✅ Via API endpoint | ❌ Not needed |
| Tax Calculator | ✅ Banker's Rounding | ✅ Via API endpoint | ❌ Not needed |
| Content Router | ✅ Classification logic | ✅ Via API endpoint | ❌ Not needed |
| API Endpoint | ❌ Not applicable | ✅ Full pipeline | ✅ Happy path |
| Database Persistence | ❌ Not applicable | ✅ In-memory DB | ✅ Real DB |
| React UI | ❌ Not applicable | ❌ Not applicable | ✅ Full workflow |

### Sample Email Fixtures (PRD v0.3 Section 13)

**Requirement**: Store sample emails from project brief in `/fixtures/` directory for E2E tests.

**Fixtures**:
1. `sample-email-1-expense.txt`: Expense email example from brief
2. `sample-email-2-other.txt`: Non-expense email example from brief

**Usage in E2E Tests**:
```typescript
import { readFileSync } from 'fs';

const sampleEmail1 = readFileSync('./fixtures/sample-email-1-expense.txt', 'utf-8');

test('sample email 1 parses as expense', async ({ page }) => {
  await page.fill('[data-testid="content-input"]', sampleEmail1);
  await page.click('[data-testid="submit-button"]');
  await expect(page.locator('[data-testid="classification"]')).toHaveText('expense');
});
```

## Alternatives Considered

### Alternative 1: Unit Tests Only

**Approach**: Only write unit tests for domain logic, skip integration and E2E tests.

**Pros**:
- Faster test execution
- Less test code to maintain
- Simpler CI setup

**Cons**:
- **Misses integration bugs**: API contract issues, DB persistence errors not caught
- No confidence in full pipeline
- No UI testing (user experience not validated)

**Rejected because**: Integration and E2E tests are critical for validating full system behavior.

### Alternative 2: E2E Tests Only

**Approach**: Only write E2E tests, skip unit and integration tests.

**Pros**:
- Tests real user workflows
- Validates entire system

**Cons**:
- **Slow**: E2E tests take minutes to run (poor TDD feedback loop)
- **Brittle**: Flaky tests due to timing issues, network delays
- **Poor failure diagnostics**: Hard to identify root cause of failures
- No isolation (tests depend on external services)

**Rejected because**: Unit tests are essential for fast feedback and precise failure diagnosis.

### Alternative 3: Manual Testing Only

**Approach**: No automated tests, rely on manual QA.

**Pros**:
- No test code to maintain
- Flexible (exploratory testing)

**Cons**:
- **Not repeatable**: Manual tests not run consistently
- **Doesn't scale**: Takes hours to manually test all scenarios
- **No regression prevention**: Changes break existing functionality without detection

**Rejected because**: Automated tests are essential for CI/CD and regression prevention.

### Alternative 4: Snapshot Testing (React)

**Approach**: Use Jest snapshot tests for React components.

**Pros**:
- Easy to write (auto-generate snapshots)
- Catches unintended UI changes

**Cons**:
- **Brittle**: Snapshots break on minor changes (noisy)
- **Poor test quality**: Doesn't validate behavior, only DOM structure
- Encourages "approve all changes" without review

**Rejected because**: Playwright E2E tests provide better validation of user workflows.

## Implementation

### Test Project Structure

```
/tests
  /FlowinglyParsingService.UnitTests
    /Domain
      TagValidatorTests.cs
      TaxCalculatorTests.cs
      NormalizerTests.cs
    /Application
      ContentRouterTests.cs
  /FlowinglyParsingService.IntegrationTests
    /Api
      ParseEndpointTests.cs
    /Database
      RepositoryTests.cs
  /FlowinglyParsingService.E2ETests
    /tests
      expense-workflow.spec.ts
      other-workflow.spec.ts
/fixtures
  sample-email-1-expense.txt
  sample-email-2-other.txt
```

### BDD Test Format (Given/When/Then Comments)

```csharp
[Fact]
public void TagValidator_OverlappingTags_ReturnsError()
{
    // GIVEN: Content with overlapping tags <a><b></a></b>
    var content = "<a><b></a></b>";
    var validator = new TagValidator();

    // WHEN: Validating content
    var result = validator.Validate(content);

    // THEN: Validation fails with specific error
    result.IsValid.Should().BeFalse();
    result.Error.Should().Contain("Overlapping tags");
    result.Error.Should().Contain("expected </b>, found </a>");
}
```

### CI/CD Pipeline

```yaml
# .github/workflows/test.yml
name: Test Suite

on: [push, pull_request]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Unit Tests
        run: dotnet test tests/FlowinglyParsingService.UnitTests

  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Integration Tests
        run: dotnet test tests/FlowinglyParsingService.IntegrationTests

  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Install Playwright
        run: npx playwright install
      - name: Run E2E Tests
        run: npm run test:e2e
```

### Coverage Targets

**Unit Tests**: 80%+ code coverage
- Domain logic (parsers, validators, calculators): 90%+
- Application logic (commands, queries, handlers): 70%+

**Integration Tests**: 60%+ coverage
- API endpoints: 80%+ (all endpoints, happy + error paths)
- Database operations: 60%+ (CRUD + edge cases)

**E2E Tests**: Happy path + critical error scenarios
- Expense workflow (Sample Email 1): ✅
- Other workflow (Sample Email 2): ✅
- Validation error (unclosed tag): ✅
- Tax calculation (Banker's Rounding): ✅

## Testing Tools

### Backend (.NET)
- **xUnit**: Test framework
- **FluentAssertions**: Readable assertions (`result.Should().Be(expected)`)
- **WebApplicationFactory**: ASP.NET Core integration testing
- **Moq** (if needed): Mocking external dependencies

### Frontend (React)
- **Playwright**: E2E testing framework
- **TypeScript**: Type-safe test code
- **Fixtures**: Sample emails from project brief

### Coverage Reporting
- **Coverlet**: Code coverage for .NET
- **ReportGenerator**: HTML coverage reports
- **CI Integration**: Upload coverage to Codecov or similar (optional)

## References

- PRD + Technical Specification v0.3, Section 13: Testing Strategy
- PRD v0.3, Section 6: Document History (6 new BDD scenarios)
- ADR-0008: Parsing and Validation Rules (overlapping tags, tax precedence, ambiguous time)
- ADR-0009: Banker's Rounding (rounding policy test cases)
- xUnit Documentation: https://xunit.net/
- FluentAssertions: https://fluentassertions.com/
- Playwright: https://playwright.dev/
- ASP.NET Core Integration Testing: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

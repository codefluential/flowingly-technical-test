# Test Strategy — M1 Core Parsing & Validation

> Comprehensive test organization, naming conventions, and TDD workflow for 30+ unit tests across 7 domain components.

**Version**: 1.0
**Created**: 2025-10-06
**Last Updated**: 2025-10-06

---

## Table of Contents

1. [Test Organization](#1-test-organization)
2. [Naming Conventions](#2-naming-conventions)
3. [TDD Workflow (RED-GREEN-REFACTOR)](#3-tdd-workflow-red-green-refactor)
4. [Test Coverage Targets](#4-test-coverage-targets)
5. [FluentAssertions Patterns](#5-fluentassertions-patterns)
6. [Test Fixture Usage](#6-test-fixture-usage)
7. [Additional Guidelines](#7-additional-guidelines)

---

## 1. Test Organization

### Directory Structure

Tests are organized to **mirror the application architecture** (Clean/Hexagonal):

```
tests/Flowingly.ParsingService.UnitTests/
├── Domain/                           # Domain logic tests (80%+ coverage target)
│   ├── TagValidatorTests.cs          # 7+ tests
│   ├── NumberNormalizerTests.cs      # 4+ tests
│   ├── TaxCalculatorTests.cs         # 10+ tests
│   ├── TimeParserTests.cs            # 5+ tests
│   └── XmlIslandExtractorTests.cs    # 4+ tests
├── Application/                      # Application logic tests (70%+ coverage target)
│   ├── ContentRouterTests.cs         # 5+ tests
│   └── ExpenseProcessorTests.cs      # 6+ tests
├── fixtures/                         # Test data files
│   ├── sample-email-1-expense.txt    # From task_011
│   └── sample-email-2-other.txt      # From task_011
├── TEST_STRATEGY.md                  # This document
└── Flowingly.ParsingService.UnitTests.csproj
```

### File Naming Conventions

- **Pattern**: `{ComponentName}Tests.cs`
- **Examples**:
  - `TagValidator` → `TagValidatorTests.cs`
  - `TaxCalculator` → `TaxCalculatorTests.cs`
  - `ContentRouter` → `ContentRouterTests.cs`

### Test Class Organization

```csharp
namespace Flowingly.ParsingService.UnitTests.Domain;

public class TagValidatorTests
{
    // Group 1: Happy path tests
    [Fact]
    public void Validate_ProperlyNestedTags_ReturnsValid() { }

    [Fact]
    public void Validate_SingleTag_ReturnsValid() { }

    // Group 2: Error scenarios
    [Fact]
    public void Validate_UnclosedTag_ThrowsValidationException() { }

    [Fact]
    public void Validate_OverlappingTags_ThrowsValidationException() { }

    // Group 3: Edge cases
    [Fact]
    public void Validate_EmptyString_ReturnsValid() { }

    [Fact]
    public void Validate_NoTags_ReturnsValid() { }
}
```

**Organizational Principles**:
- Group related tests together (happy path, errors, edge cases)
- Order tests from simple to complex
- Use descriptive method names (see Naming Conventions)
- One assertion per test when possible (focused tests)

---

## 2. Naming Conventions

### Three Recommended Patterns

We support **three naming patterns**. Choose the one that best fits the test scenario:

#### Pattern 1: MethodUnderTest_Scenario_ExpectedResult

**Best for**: Focused unit tests where the method under test is clear.

**Format**: `{MethodName}_{Scenario}_{ExpectedResult}`

**Examples**:
```csharp
Validate_OverlappingTags_ThrowsValidationException
Validate_ProperlyNestedTags_ReturnsValid
Calculate_InclusiveTotalWithGst_ReturnsTaxBreakdown
Parse_ValidTimeString_ReturnsDateTime
Extract_ValidXmlIsland_ReturnsExpenseData
Route_ExpenseContent_ReturnsExpenseClassification
Process_ValidExpenseEmail_ReturnsExpenseResult
```

#### Pattern 2: Given_When_Then (BDD Style)

**Best for**: Behavior-driven tests focusing on business scenarios.

**Format**: `Given_{Precondition}_When_{Action}_Then_{ExpectedOutcome}`

**Examples**:
```csharp
Given_OverlappingTags_When_Validating_Then_ThrowsException
Given_ValidNesting_When_Validating_Then_ReturnsValid
Given_InclusiveTotal_When_CalculatingTax_Then_ComputesGstBreakdown
Given_AmbiguousTime_When_Parsing_Then_ReturnsNull
Given_MalformedXml_When_Extracting_Then_ThrowsException
Given_ExpenseContent_When_Routing_Then_ReturnsExpenseClassification
Given_MissingTotalTag_When_Processing_Then_ThrowsValidationException
```

#### Pattern 3: Should_X_When_Y (Natural Language)

**Best for**: Expressing expected behavior in plain language.

**Format**: `Should_{ExpectedBehavior}_When_{Condition}`

**Examples**:
```csharp
Should_ThrowException_When_TagsAreOverlapping
Should_ReturnValid_When_TagsAreProperlyNested
Should_ComputeTaxBreakdown_When_GivenInclusiveTotal
Should_ReturnNull_When_TimeIsAmbiguous
Should_ThrowException_When_XmlIsMalformed
Should_ClassifyAsExpense_When_ContentContainsTotalTag
Should_RejectRequest_When_TotalTagIsMissing
```

### Choosing a Pattern

- **Pattern 1**: Default choice for most unit tests (clear, concise)
- **Pattern 2**: Use when writing BDD scenarios (aligns with ADR-0010)
- **Pattern 3**: Use when natural language improves readability

**Important**: Be consistent within a test class. Don't mix patterns arbitrarily.

---

## 3. TDD Workflow (RED-GREEN-REFACTOR)

M1 uses **strict TDD cycles** for all domain components. Follow this workflow:

### RED Phase (Write Failing Test)

**Goal**: Write a test that fails because the implementation doesn't exist yet.

**Steps**:

1. **Write test method** with descriptive name following naming conventions
2. **Arrange**: Set up test data (use fixtures if applicable)
3. **Act**: Call method under test (wrap in `Action` for exception testing)
4. **Assert**: Use FluentAssertions to verify expected behavior
5. **Run test**: Verify it **FAILS** (RED) — compilation error or assertion failure
6. **Commit**: `test(component): add failing test for X`

**Example (RED Phase)**:

```csharp
[Fact]
public void Validate_OverlappingTags_ThrowsValidationException()
{
    // Arrange
    var validator = new TagValidator(); // ❌ Class doesn't exist yet
    var input = "<a><b></a></b>";

    // Act
    Action act = () => validator.Validate(input);

    // Assert
    act.Should().Throw<ValidationException>()
        .WithMessage("*OVERLAPPING_TAGS*");
}
```

**Run test**: `dotnet test --filter "FullyQualifiedName~Validate_OverlappingTags"`
**Expected**: ❌ FAILS (compilation error: `TagValidator` does not exist)

**Commit**:
```bash
git add tests/Flowingly.ParsingService.UnitTests/Domain/TagValidatorTests.cs
git commit -m "test(tag-validator): add failing test for overlapping tags validation

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### GREEN Phase (Make Test Pass)

**Goal**: Write **minimal code** to make the test pass. No over-engineering.

**Steps**:

1. **Implement minimal code** to make test pass (create class, method, logic)
2. **Run test**: Verify it **PASSES** (GREEN)
3. **Run all tests**: Ensure no regressions in other tests
4. **Commit**: `feat(component): implement X to pass tests`

**Example (GREEN Phase)**:

```csharp
// src/Domain/Validators/TagValidator.cs
public class TagValidator : ITagValidator
{
    public ValidationResult Validate(string input)
    {
        var stack = new Stack<string>();
        var tagPattern = new Regex(@"<(/?)(\w+)>");

        foreach (Match match in tagPattern.Matches(input))
        {
            var isClosingTag = match.Groups[1].Value == "/";
            var tagName = match.Groups[2].Value;

            if (isClosingTag)
            {
                if (stack.Count == 0 || stack.Peek() != tagName)
                {
                    throw new ValidationException($"OVERLAPPING_TAGS: expected </{stack.Peek()}>, found </{tagName}>");
                }
                stack.Pop();
            }
            else
            {
                stack.Push(tagName);
            }
        }

        if (stack.Count > 0)
        {
            throw new ValidationException($"UNCLOSED_TAGS: {string.Join(", ", stack)}");
        }

        return ValidationResult.Valid();
    }
}
```

**Run test**: `dotnet test --filter "FullyQualifiedName~Validate_OverlappingTags"`
**Expected**: ✅ PASSES

**Run all tests**: `dotnet test`
**Expected**: ✅ All tests pass

**Commit**:
```bash
git add src/Domain/Validators/TagValidator.cs
git commit -m "feat(tag-validator): implement stack-based validation to detect overlapping tags

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### REFACTOR Phase (Improve Code Quality)

**Goal**: Improve code quality while keeping tests green.

**Steps**:

1. **Review code** for duplication, complexity, code smells
2. **Refactor** while keeping tests green (extract methods, simplify logic)
3. **Run all tests** after each refactor to ensure they still pass
4. **Commit**: `refactor(component): improve X`

**Example (REFACTOR Phase)**:

```csharp
// Before refactor
public ValidationResult Validate(string input)
{
    var stack = new Stack<string>();
    var tagPattern = new Regex(@"<(/?)(\w+)>");

    foreach (Match match in tagPattern.Matches(input))
    {
        var isClosingTag = match.Groups[1].Value == "/";
        var tagName = match.Groups[2].Value;

        if (isClosingTag)
        {
            if (stack.Count == 0 || stack.Peek() != tagName)
            {
                throw new ValidationException($"OVERLAPPING_TAGS: expected </{stack.Peek()}>, found </{tagName}>");
            }
            stack.Pop();
        }
        else
        {
            stack.Push(tagName);
        }
    }

    if (stack.Count > 0)
    {
        throw new ValidationException($"UNCLOSED_TAGS: {string.Join(", ", stack)}");
    }

    return ValidationResult.Valid();
}

// After refactor: Extract methods for clarity
public ValidationResult Validate(string input)
{
    var stack = new Stack<string>();
    var matches = ExtractTagMatches(input);

    foreach (var match in matches)
    {
        ProcessTag(match, stack);
    }

    ValidateStackIsEmpty(stack);

    return ValidationResult.Valid();
}

private IEnumerable<Match> ExtractTagMatches(string input)
{
    var tagPattern = new Regex(@"<(/?)(\w+)>");
    return tagPattern.Matches(input).Cast<Match>();
}

private void ProcessTag(Match match, Stack<string> stack)
{
    var isClosingTag = match.Groups[1].Value == "/";
    var tagName = match.Groups[2].Value;

    if (isClosingTag)
    {
        ValidateClosingTag(tagName, stack);
        stack.Pop();
    }
    else
    {
        stack.Push(tagName);
    }
}

private void ValidateClosingTag(string tagName, Stack<string> stack)
{
    if (stack.Count == 0 || stack.Peek() != tagName)
    {
        var expected = stack.Count > 0 ? stack.Peek() : "none";
        throw new ValidationException($"OVERLAPPING_TAGS: expected </{expected}>, found </{tagName}>");
    }
}

private void ValidateStackIsEmpty(Stack<string> stack)
{
    if (stack.Count > 0)
    {
        throw new ValidationException($"UNCLOSED_TAGS: {string.Join(", ", stack)}");
    }
}
```

**Run all tests**: `dotnet test`
**Expected**: ✅ All tests still pass

**Commit**:
```bash
git add src/Domain/Validators/TagValidator.cs
git commit -m "refactor(tag-validator): extract methods to improve readability

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### TDD Cycle Summary

```
┌─────────────────────────────────────────────────────────────┐
│                       TDD CYCLE                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. RED:    Write failing test                             │
│             ↓                                               │
│             ❌ Test FAILS (compilation or assertion)        │
│             ↓                                               │
│             Commit: "test(component): add failing test..."  │
│             ↓                                               │
│  2. GREEN:  Write minimal implementation                    │
│             ↓                                               │
│             ✅ Test PASSES                                  │
│             ↓                                               │
│             Commit: "feat(component): implement..."         │
│             ↓                                               │
│  3. REFACTOR: Improve code quality                          │
│             ↓                                               │
│             ✅ Tests still pass                             │
│             ↓                                               │
│             Commit: "refactor(component): improve..."       │
│             ↓                                               │
│  4. REPEAT: Next test in TDD cycle                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. Test Coverage Targets

### Per-Component Test Counts

Based on task decomposition (task_012.json):

| Component | Test Count | Focus Areas |
|-----------|------------|-------------|
| **TagValidator** | 7+ tests | Overlapping tags, unclosed tags, proper nesting, edge cases (empty string, no tags, deeply nested) |
| **NumberNormalizer** | 4+ tests | Currency symbols ($, NZD), comma separators, decimal precision, invalid formats |
| **TaxCalculator** | 10+ tests | Banker's Rounding edge cases (0.5 → even), GST calculation, precision (2 decimal places), zero total, negative total |
| **TimeParser** | 5+ tests | Various formats (HH:MM, HHMM, H:MM), ambiguous times (230 → 2:30 or 23:0?), invalid inputs (25:00, 99:99), null handling |
| **XmlIslandExtractor** | 4+ tests | Valid XML parsing, security checks (DTD/XXE disabled), malformed XML, missing tags |
| **ContentRouter** | 5+ tests | Expense classification (has `<total>` tag), other classification (no `<total>` tag), edge cases (empty content, only text) |
| **ExpenseProcessor** | 6+ tests | Full pipeline with fixtures (sample-email-1-expense.txt), validation (missing `<total>`), error handling, tax breakdown verification |

**Total**: 30+ unit tests (exceeds M1 requirement)

### Coverage Percentage Targets

Based on ADR-0010:

- **Domain Logic**: 80%+ code coverage
  - Validators: 90%+ (critical path)
  - Calculators: 90%+ (financial logic)
  - Parsers: 80%+
  - Extractors: 80%+

- **Application Logic**: 70%+ code coverage
  - Routers: 80%+
  - Processors: 70%+ (uses integration tests for full pipeline)

**Measurement**:
```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# View coverage summary
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```

---

## 5. FluentAssertions Patterns

### Why FluentAssertions?

FluentAssertions provides **readable, expressive assertions** that improve test clarity:

```csharp
// ❌ Traditional xUnit assertions (avoid)
Assert.Equal(expected, actual);
Assert.True(result.IsValid);
Assert.Throws<ValidationException>(() => validator.Validate(input));

// ✅ FluentAssertions (use this)
actual.Should().Be(expected);
result.IsValid.Should().BeTrue();
validator.Invoking(v => v.Validate(input))
    .Should().Throw<ValidationException>();
```

### Common Assertion Patterns

#### 1. Exception Assertions

**Use case**: Testing validation errors, business rule violations.

```csharp
[Fact]
public void Validate_UnclosedTag_ThrowsValidationException()
{
    // Arrange
    var validator = new TagValidator();
    var input = "<total>120";

    // Act
    Action act = () => validator.Validate(input);

    // Assert
    act.Should().Throw<ValidationException>()
        .WithMessage("*UNCLOSED_TAGS*");
}
```

**Pattern variations**:
```csharp
// Exact message match
act.Should().Throw<ValidationException>()
    .WithMessage("UNCLOSED_TAGS: total");

// Wildcard pattern match
act.Should().Throw<ValidationException>()
    .WithMessage("*UNCLOSED_TAGS*");

// Message contains substring
act.Should().Throw<ValidationException>()
    .And.Message.Should().Contain("UNCLOSED_TAGS");

// Multiple exception types
act.Should().ThrowExactly<ArgumentNullException>();
```

#### 2. Equality Assertions (Decimal Precision)

**Use case**: Testing tax calculations with Banker's Rounding.

```csharp
[Fact]
public void Calculate_InclusiveTotalWithGst_UsesBankersRounding()
{
    // Arrange
    var calculator = new TaxCalculator();
    var totalInclTax = 120.50m;
    var taxRate = 0.15m;

    // Act
    var result = calculator.Calculate(totalInclTax, taxRate);

    // Assert (Banker's Rounding: 104.78260869... → 104.78)
    result.TotalExclTax.Should().Be(104.78m);
    result.SalesTax.Should().Be(15.72m);
    result.TotalInclTax.Should().Be(120.50m);
}
```

**Pattern variations**:
```csharp
// Exact decimal equality
result.TotalExclTax.Should().Be(104.78m);

// Approximate equality (floating point)
result.TotalExclTax.Should().BeApproximately(104.78m, 0.01m);

// Comparison operators
result.TotalExclTax.Should().BeGreaterThan(100m);
result.TotalExclTax.Should().BeLessThanOrEqualTo(105m);
```

#### 3. Null Checks

**Use case**: Testing nullable return values (e.g., ambiguous time parsing).

```csharp
[Fact]
public void Parse_AmbiguousTime_ReturnsNull()
{
    // Arrange
    var parser = new TimeParser();
    var input = "230"; // Ambiguous: 2:30 or 23:0?

    // Act
    var result = parser.Parse(input);

    // Assert
    result.Should().BeNull();
}
```

**Pattern variations**:
```csharp
// Not null
result.Should().NotBeNull();

// Null
result.Should().BeNull();

// Null or empty (strings)
result.Should().BeNullOrEmpty();
```

#### 4. Type Checks

**Use case**: Testing polymorphic returns or factory methods.

```csharp
[Fact]
public void Route_ExpenseContent_ReturnsExpenseClassification()
{
    // Arrange
    var router = new ContentRouter();
    var content = "Expense for <total>120.50</total>";

    // Act
    var result = router.Route(content);

    // Assert
    result.Should().BeOfType<ExpenseClassification>();
    result.ClassificationType.Should().Be("expense");
}
```

**Pattern variations**:
```csharp
// Exact type
result.Should().BeOfType<ExpenseResult>();

// Assignable to type (inheritance)
result.Should().BeAssignableTo<IClassification>();

// Not of type
result.Should().NotBeOfType<OtherClassification>();
```

#### 5. Collection Assertions

**Use case**: Testing error lists, parsed items, validation results.

```csharp
[Fact]
public void Process_ValidExpense_ReturnsNoErrors()
{
    // Arrange
    var processor = new ExpenseProcessor();
    var content = "<total>120.50</total><vendor>Mojo Coffee</vendor>";

    // Act
    var result = processor.Process(content);

    // Assert
    result.Errors.Should().BeEmpty();
    result.Expense.Should().NotBeNull();
}
```

**Pattern variations**:
```csharp
// Empty collection
result.Errors.Should().BeEmpty();

// Not empty
result.Items.Should().NotBeEmpty();

// Count
result.Items.Should().HaveCount(3);

// Contains specific item
result.Tags.Should().Contain("total");

// All items match condition
result.Amounts.Should().AllSatisfy(a => a.Should().BeGreaterThan(0));
```

#### 6. String Assertions

**Use case**: Testing parsed text, error messages, formatted output.

```csharp
[Fact]
public void Normalize_NumberWithCommas_RemovesCommas()
{
    // Arrange
    var normalizer = new NumberNormalizer();
    var input = "35,000";

    // Act
    var result = normalizer.Normalize(input);

    // Assert
    result.Should().Be("35000");
    result.Should().NotContain(",");
}
```

**Pattern variations**:
```csharp
// Exact match
result.Should().Be("expected");

// Contains substring
result.Should().Contain("expense");

// Starts/ends with
result.Should().StartWith("Error:");
result.Should().EndWith("failed");

// Case-insensitive
result.Should().BeEquivalentTo("EXPECTED");

// Regex match
result.Should().MatchRegex(@"\d{3}-\d{3}-\d{4}");
```

---

## 6. Test Fixture Usage

### What Are Fixtures?

Fixtures are **pre-created test data files** that represent realistic inputs. Task_011 created two fixtures:

- `fixtures/sample-email-1-expense.txt`: Realistic expense email (from project brief)
- `fixtures/sample-email-2-other.txt`: Non-expense email (from project brief)

### Why Use Fixtures?

1. **Realistic Testing**: Test with actual email formats from requirements
2. **Reusability**: Same fixtures used across unit, integration, and E2E tests
3. **Maintainability**: Update fixture once, all tests benefit
4. **Documentation**: Fixtures serve as living examples of expected input

### How to Load Fixtures

```csharp
// Option 1: Load from fixtures/ directory (project root)
var sampleEmail = File.ReadAllText("fixtures/sample-email-1-expense.txt");

// Option 2: Load from relative path (if test project structure differs)
var sampleEmail = File.ReadAllText("../../../fixtures/sample-email-1-expense.txt");

// Option 3: Use Path.Combine for cross-platform compatibility
var fixturesPath = Path.Combine(Directory.GetCurrentDirectory(), "fixtures");
var sampleEmail = File.ReadAllText(Path.Combine(fixturesPath, "sample-email-1-expense.txt"));
```

### Example: Using Fixtures in Tests

#### Sample Email 1 (Expense):

**Fixture content** (`fixtures/sample-email-1-expense.txt`):
```
Hi Patricia,

Please create an expense claim for the below. Relevant details are marked up as requested…

<expense><cost_centre>DEV632</cost_centre><total>35,000</total><payment_method>personal card</payment_method></expense>

From: William Steele
Sent: Friday, 16 June 2022 10:32 AM
To: Maria Washington
Subject: test
```

**Test using fixture**:

```csharp
[Fact]
public void Process_SampleEmail1_ParsesExpenseCorrectly()
{
    // Arrange
    var processor = new ExpenseProcessor();
    var sampleEmail = File.ReadAllText("fixtures/sample-email-1-expense.txt");

    // Act
    var result = processor.Process(sampleEmail);

    // Assert
    result.Should().NotBeNull();
    result.Classification.Should().Be("expense");
    result.Expense.CostCentre.Should().Be("DEV632");
    result.Expense.Total.Should().Be(35000m);
    result.Expense.PaymentMethod.Should().Be("personal card");
}
```

#### Sample Email 2 (Other/Unprocessed):

**Test using fixture**:

```csharp
[Fact]
public void Process_SampleEmail2_ClassifiesAsOther()
{
    // Arrange
    var processor = new ExpenseProcessor();
    var sampleEmail = File.ReadAllText("fixtures/sample-email-2-other.txt");

    // Act
    var result = processor.Process(sampleEmail);

    // Assert
    result.Should().NotBeNull();
    result.Classification.Should().Be("other");
    result.Other.Should().NotBeNull();
    result.Expense.Should().BeNull();
}
```

### Fixture Best Practices

1. **Don't modify fixtures in tests**: Keep fixtures read-only
2. **Use fixtures for integration tests**: Verify full pipeline with realistic data
3. **Create additional fixtures as needed**: Add edge case fixtures for specific scenarios
4. **Document fixture purpose**: Add comments explaining what each fixture tests
5. **Commit fixtures to version control**: Ensure all developers have same test data

---

## 7. Additional Guidelines

### AAA Pattern (Arrange-Act-Assert)

All tests MUST follow the AAA structure:

```csharp
[Fact]
public void ExampleTest()
{
    // Arrange: Set up test data and dependencies
    var input = "test input";
    var expectedOutput = "expected result";
    var component = new ComponentUnderTest();

    // Act: Execute the method under test
    var result = component.MethodUnderTest(input);

    // Assert: Verify the expected outcome
    result.Should().Be(expectedOutput);
}
```

### Test Categorization (Optional)

Use `[Trait]` attributes to categorize tests:

```csharp
[Fact]
[Trait("Category", "Unit")]
[Trait("Component", "TagValidator")]
public void Validate_OverlappingTags_ThrowsValidationException()
{
    // Test implementation
}
```

**Run tests by category**:
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Component=TagValidator"
```

### Parameterized Tests with [Theory]

Use `[Theory]` with `[InlineData]` for testing multiple inputs:

```csharp
[Theory]
[InlineData("<a><b></b></a>", true)]       // Valid nesting
[InlineData("<a><b></a></b>", false)]      // Overlapping
[InlineData("<total>120</total>", true)]   // Valid single tag
[InlineData("<total>120", false)]          // Unclosed
public void Validate_VariousInputs_ValidatesCorrectly(string input, bool expectedValid)
{
    // Arrange
    var validator = new TagValidator();

    // Act
    var result = validator.Validate(input);

    // Assert
    result.IsValid.Should().Be(expectedValid);
}
```

### Test Data Builders (Optional)

For complex domain objects, create test data builders:

```csharp
public class ExpenseBuilder
{
    private string _vendor = "Default Vendor";
    private decimal _total = 100.00m;
    private string _costCentre = "UNKNOWN";

    public ExpenseBuilder WithVendor(string vendor)
    {
        _vendor = vendor;
        return this;
    }

    public ExpenseBuilder WithTotal(decimal total)
    {
        _total = total;
        return this;
    }

    public ExpenseBuilder WithCostCentre(string costCentre)
    {
        _costCentre = costCentre;
        return this;
    }

    public Expense Build()
    {
        return new Expense
        {
            Vendor = _vendor,
            Total = _total,
            CostCentre = _costCentre
        };
    }
}

// Usage in tests
[Fact]
public void ExampleTest()
{
    var expense = new ExpenseBuilder()
        .WithVendor("Mojo Coffee")
        .WithTotal(120.50m)
        .Build();

    // Use expense in test
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~TagValidatorTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~Validate_OverlappingTags"

# Run with verbosity
dotnet test --logger "console;verbosity=detailed"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### CI/CD Integration

Tests will run automatically in GitHub Actions:

```yaml
# .github/workflows/test.yml
- name: Run Unit Tests
  run: dotnet test tests/Flowingly.ParsingService.UnitTests
```

---

## Summary

This test strategy establishes:

✅ **Clear organization**: Tests mirror architecture (Domain/, Application/)
✅ **Consistent naming**: 3 patterns (MethodUnderTest_Scenario_Expected, Given_When_Then, Should_X_When_Y)
✅ **Disciplined workflow**: TDD RED-GREEN-REFACTOR with commit messages
✅ **Coverage targets**: 30+ tests, 80%+ domain coverage, 70%+ application coverage
✅ **Readable assertions**: FluentAssertions patterns for all scenarios
✅ **Realistic data**: Fixtures from task_011 for integration testing

**Next Steps**: Task_014 begins TDD RED phase for TagValidator (7+ tests).

---

**References**:
- ADR-0010: Test Strategy and Coverage
- PRD v0.3, Section 13: Testing Strategy
- FluentAssertions: https://fluentassertions.com/
- xUnit: https://xunit.net/

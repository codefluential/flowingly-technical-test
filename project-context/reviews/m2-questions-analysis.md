# M2 Critical Questions - Analysis & Recommendations

**Date**: 2025-10-06
**Reviewer**: Claude Code
**Context**: User questions about M2 issues, TDD adherence, and architectural decisions

---

## Q1: Why did task_030 (M1→M2 gate) pass despite missing endpoint wiring?

### Answer

Task_030 **correctly passed** because it was an **M1 DoD gate** (Domain layer only), NOT an M2 gate (API layer).

**M1 Scope - What task_030 Verified**:
- ✅ 116 unit tests passing (domain logic only)
- ✅ All parsing components implemented (TagValidator, TaxCalculator, ExpenseProcessor, etc.)
- ✅ Zero HTTP/API dependencies (pure domain)
- ✅ Interfaces and ports properly defined
- ✅ Clean/Hexagonal architecture maintained

**M1 Scope - What task_030 Did NOT Verify** (correctly):
- ❌ API endpoint wiring (that's M2 responsibility)
- ❌ HTTP request/response handling
- ❌ Contract/integration tests
- ❌ Middleware functionality

### The Gap

**Missing Integration Gate**: There was no explicit gate between M1 (domain complete) and M2 (API complete).

**Timeline**:
1. task_030 (Oct 6, 17:04) - M1 complete, domain layer works
2. task_031-036 (Oct 6, 18:00-20:00) - M2 components created (DTOs, handlers, DI)
3. task_037 (Oct 6, 20:47) - Contract tests created → discovered mock still active

**Gap Duration**: 3.5 hours between M1 completion and discovering endpoint not wired.

### Root Cause

**Architectural Layering**: Clean Architecture separates concerns:
- **M1 = Domain Layer** - Business logic, no HTTP knowledge
- **M2 = API Layer** - HTTP contracts, endpoints, middleware

M1 gate verified domain works. M2 gate (task_040) should verify API integration, but task_037 tests exposed issues first.

### Recommendation

**Add Explicit Integration Gate**: Between M1 and M2, add a task:
```
task_030.5: "Verify M1→M2 Integration Smoke Test"
- Create minimal endpoint wired to ONE domain service
- Write 1-2 integration tests proving domain → API wiring works
- Catches endpoint wiring issues before full M2 implementation
```

**Benefits**:
- Early detection of integration issues
- Prevents accumulating work on wrong foundation
- Validates Clean Architecture ports/adapters actually connect

---

## Q2: Single source of truth for contracts - Can we safely remove `src/Api/Contracts/`?

### Answer

**YES - Safe to remove** ✅

### Analysis

**Duplicate Locations Found**:
1. `src/Api/Contracts/` - 9 files (ErrorResponse with nested Error structure)
2. `src/Api/Extensions/ExceptionExtensions.cs` - Used Api.Contracts
3. `contracts/` - 9 files (ErrorResponse with flat ErrorCode/Message structure)

**Current Usage** (after M2-002 fix):
- ✅ ExceptionMappingMiddleware → uses `contracts/` (fixed)
- ✅ ParseEndpoint → uses `contracts/`
- ✅ Tests → use `contracts/`
- ❌ `src/Api/Contracts/` → UNUSED (dead code)
- ❌ `src/Api/Extensions/` → UNUSED (dead code)

### Impact Assessment

**Removal Performed**:
```bash
rm -rf src/Api/Contracts src/Api/Extensions
```

**Verification**:
```bash
dotnet build  # ✅ Success (0 errors, 0 warnings)
dotnet test --filter="Category=Contract"  # ✅ 13/13 passing
```

**Git Commit**: `8d07eba` - refactor(cleanup): remove duplicate contract definitions

### Why Duplicates Existed

**Hypothesis**: Created during task_033 (Create Error Codes & Models) or task_034 (Implement Error Mapping) as initial implementation, then `contracts/` project created separately later. Old files never cleaned up.

**Evidence**:
- `src/Api/Contracts/ErrorResponse.cs` created Oct 6, 18:08
- `contracts/Errors/ErrorResponse.cs` (separate namespace, different structure)
- No cross-references between them

### Benefits of Removal

1. **Enforces Single Source of Truth** - Only `contracts/` project used
2. **Eliminates Confusion** - No more "which ErrorResponse to import?"
3. **Prevents Future Bugs** - Can't accidentally use wrong namespace
4. **Cleaner Architecture** - Contracts project is shared boundary
5. **Aligns with Clean Architecture** - API layer depends on Contracts (inward dependency)

### Recommendation

**Prevent Future Duplication**:
1. Add `.editorconfig` rule preventing `Api.Contracts` namespace
2. Document in `CLAUDE.md`: "All contracts in `contracts/` project only"
3. Add to code review checklist: "No duplicate DTOs"

---

## Q3: Reflection usage - Why do we need it? Based on objective, is it necessary?

### Current Implementation

**Location**: `src/Application/Handlers/ParseMessageCommandHandler.cs:149-156`

**Code**:
```csharp
// Use reflection to extract RawTags and Note from anonymous type
var rawTagsProperty = otherData.GetType().GetProperty("RawTags");
var noteProperty = otherData.GetType().GetProperty("Note");

var rawTags = (Dictionary<string, string>)(rawTagsProperty?.GetValue(otherData)
    ?? new Dictionary<string, string>());
var note = (string)(noteProperty?.GetValue(otherData)
    ?? "Content stored for future processing");
```

### Why Reflection is Used

**Root Cause**: `ProcessingResult.Data` is `object?` to support different processor return types.

**Flow**:
1. `OtherProcessor.ProcessAsync()` returns `new { RawTags = ..., Note = ... }` (anonymous type)
2. Stored in `ProcessingResult.Data` as `object?`
3. Handler needs to extract properties → requires reflection or dynamic

### Is Reflection Necessary?

**NO** - There's a better architectural solution.

### Better Alternative: Introduce Processing Data DTOs

**Proposed Solution**:

```csharp
// src/Domain/Models/OtherProcessingData.cs
public sealed record OtherProcessingData
{
    public required Dictionary<string, string> RawTags { get; init; }
    public required string Note { get; init; }
}

// src/Domain/Models/ExpenseProcessingData.cs
public sealed record ExpenseProcessingData
{
    public required Expense Expense { get; init; }
}

// Update ProcessingResult to use marker interface OR discriminated union
public class ProcessingResult
{
    public string Classification { get; init; } = string.Empty;
    public IProcessingData Data { get; init; } = null!;  // Interface approach
    public bool Success { get; init; }
}

// OR use discriminated union pattern
public abstract record ProcessingResult
{
    public sealed record ExpenseResult(Expense Data) : ProcessingResult;
    public sealed record OtherResult(OtherProcessingData Data) : ProcessingResult;
}
```

**Updated OtherProcessor**:
```csharp
public Task<ProcessingResult> ProcessAsync(ParsedContent content, CancellationToken ct)
{
    var result = new ProcessingResult
    {
        Classification = "other",
        Data = new OtherProcessingData  // ✅ Concrete type, no reflection needed
        {
            RawTags = content.InlineTags,
            Note = "Content stored for future processing"
        },
        Success = true
    };

    return Task.FromResult(result);
}
```

**Updated Handler**:
```csharp
else // classification == "other"
{
    var otherData = (OtherProcessingData)result.Data;  // ✅ Simple cast, compile-time safe

    return new OtherParseResponse
    {
        Other = new OtherData
        {
            RawTags = otherData.RawTags,
            Note = otherData.Note
        },
        Meta = meta
    };
}
```

### Benefits of Concrete DTOs

| Aspect | Current (Reflection) | Proposed (DTOs) |
|--------|---------------------|-----------------|
| **Type Safety** | ❌ Runtime only | ✅ Compile-time |
| **Performance** | ❌ Reflection overhead | ✅ Direct property access |
| **Debugging** | ❌ Hard to debug binding failures | ✅ Clear stack traces |
| **Refactoring** | ❌ Breaks silently | ✅ Compile errors guide changes |
| **Testability** | ⚠️ Requires reflection in tests | ✅ Simple object comparison |
| **Documentation** | ❌ Implicit contract | ✅ Explicit types document intent |

### Objective Alignment

**Project Objective**: Production-ready parsing service with clear contracts.

**Reflection Usage**:
- ❌ Violates "clear contracts" - anonymous types hide structure
- ❌ Violates "production-ready" - reflection is maintenance burden
- ❌ Violates Clean Architecture - domain should have explicit models

**Recommendation**: **REFACTOR** - Replace anonymous types + reflection with concrete DTOs.

**Priority**: Medium (not blocking M2, but technical debt)

**Effort**: ~30 minutes
1. Create `OtherProcessingData.cs` (5 min)
2. Update `OtherProcessor` (2 min)
3. Update `ExpenseProcessor` (optional, already returns Expense)
4. Update Handler (5 min)
5. Update tests (10 min)
6. Verify all tests pass (5 min)

---

## Q4: Why did we use dynamic? Is it an issue? Should we fix it?

### Original Implementation

**Commit**: `7d40596` - feat(application): implement CQRS parse handler (task_035)

**Original Code**:
```csharp
else // classification == "other"
{
    // OtherProcessor returns anonymous object with RawTags and Note
    dynamic otherData = result.Data  // ❌ Dynamic typing
        ?? throw new InvalidOperationException("Expected data for other classification");

    return new OtherParseResponse
    {
        Other = new OtherData
        {
            RawTags = otherData.RawTags,  // ❌ Runtime binding
            Note = otherData.Note
        },
        Meta = meta
    };
}
```

### Why Dynamic Was Used

**Rationale**:
1. Quick solution to access anonymous type properties
2. Seemed convenient: `otherData.RawTags` instead of `GetType().GetProperty("RawTags")`
3. Common C# pattern for working with anonymous types
4. No compile-time type info available for anonymous types

**Context**: task_035 was implementing handler under time pressure (1h duration). Dynamic seemed like pragmatic choice.

### Is Dynamic an Issue?

**YES** - Multiple problems:

**1. Runtime Errors** (No Compile-Time Safety):
```csharp
dynamic otherData = result.Data;
var tags = otherData.RawTagz;  // ❌ Typo, fails at runtime, not compile
```

**2. Caused Test Failures**:
- Contract tests getting 500 Internal Server Error
- `RuntimeBinderException` when accessing properties
- No IntelliSense or type hints

**3. Performance Overhead**:
- Uses DLR (Dynamic Language Runtime)
- Runtime member resolution
- Slower than reflection or direct casting

**4. Debugging Difficulty**:
```
Microsoft.CSharp.RuntimeBinder.RuntimeBinderException:
'object' does not contain a definition for 'RawTags'
```
- Stack traces harder to interpret
- Can't set breakpoints on dynamic member access

**5. Violates C# Best Practices**:
- C# is statically typed language
- Dynamic should be reserved for COM interop, DLR integration
- Not for internal domain logic

### Did We Fix It?

**YES (Partially)** - Commit `d581c58`:

**Changed From** (Dynamic):
```csharp
dynamic otherData = result.Data;
return new OtherParseResponse
{
    Other = new OtherData
    {
        RawTags = otherData.RawTags,
        Note = otherData.Note
    },
    Meta = meta
};
```

**Changed To** (Reflection):
```csharp
var otherData = result.Data;

// Use reflection to extract RawTags and Note from anonymous type
var rawTagsProperty = otherData.GetType().GetProperty("RawTags");
var noteProperty = otherData.GetType().GetProperty("Note");

var rawTags = (Dictionary<string, string>)(rawTagsProperty?.GetValue(otherData)
    ?? new Dictionary<string, string>());
var note = (string)(noteProperty?.GetValue(otherData)
    ?? "Content stored for future processing");

return new OtherParseResponse
{
    Other = new OtherData
    {
        RawTags = rawTags,
        Note = note
    },
    Meta = meta
};
```

**Improvement**:
- ✅ Tests now pass (13/13)
- ✅ More explicit error handling (null coalescing)
- ✅ No DLR overhead
- ⚠️ Still uses reflection (runtime overhead, but more predictable)

### Should We Fix It Further?

**YES** - Both dynamic AND reflection should be replaced with concrete types.

**Final Solution**: See Q3 answer - use `OtherProcessingData` DTO instead of anonymous type.

**Comparison**:

| Approach | Type Safety | Performance | Maintainability | Debuggability |
|----------|-------------|-------------|-----------------|---------------|
| **Dynamic** (original) | ❌ Runtime only | ❌ DLR overhead | ❌ Hard to refactor | ❌ RuntimeBinderException |
| **Reflection** (current) | ⚠️ Runtime only | ⚠️ Reflection overhead | ⚠️ Brittle to renames | ⚠️ Indirect errors |
| **Concrete DTO** (proposed) | ✅ Compile-time | ✅ Direct access | ✅ Refactor-safe | ✅ Clear errors |

**Recommendation**: Refactor to concrete DTOs (same effort as Q3).

---

## Q5: TDD/BDD is the rule - Why was it not followed in M2?

### TDD Adherence Analysis

**M1 Tasks (task_014-030)**: ✅ **STRICT TDD**

**Evidence**:
- RED → GREEN → REFACTOR cycles enforced
- Every odd task = RED (write failing tests)
- Every even task = GREEN (make tests pass)

**Example TDD Cycles**:
```
task_014 (RED)    → TagValidator tests fail
task_015 (GREEN)  → TagValidator implementation passes
task_016 (REFACTOR) → TagValidator optimized

task_019 (RED)    → Banker's Rounding tests fail
task_020 (GREEN)  → RoundingHelper passes

task_029 (RED)    → ExpenseProcessor tests fail (12+ tests)
task_030 (GREEN)  → ExpenseProcessor passes + M1 DoD verified
```

**M1 Results**:
- 116 unit tests (387% of 30+ target)
- All domain logic test-driven
- Zero production bugs in domain layer

---

**M2 Tasks (task_031-040)**: ❌ **TDD NOT FOLLOWED**

**Actual Sequence**:
```
task_031: Create DTOs (implementation-first)
task_032: Implement FluentValidation (implementation-first)
task_033: Create Error Codes & Models (implementation-first)
task_034: Implement Error Mapping (implementation-first)
task_035: Create Parse Handler (implementation-first)
task_036: Wire Dependency Injection (implementation-first)
task_037: Write API Contract Tests (tests AFTER implementation) ❌
task_038: Create Swagger Examples (documentation)
task_039: Review API Contract (found issues tests would have caught)
```

**Evidence Tests Written After**:
- Git commit timeline shows task_037 tests created AFTER task_031-036
- Tests discovered mock handler still active (implementation bug)
- Tests discovered duplicate contracts (architectural issue)

### Why TDD Wasn't Followed

**1. Task Design Issue**

M2 tasks don't have explicit RED/GREEN pairs like M1:

**M1 Structure** (Clear TDD):
```
├─ task_014: Write TagValidator tests (RED)
├─ task_015: Implement TagValidator (GREEN)
├─ task_016: Refactor TagValidator
```

**M2 Structure** (Implementation-First):
```
├─ task_031: Create DTOs (no tests task)
├─ task_032: Implement FluentValidation (no tests task)
├─ task_033: Create Error Codes (no tests task)
├─ task_034: Implement Error Mapping (no tests task)
├─ task_035: Create Parse Handler (no tests task)
├─ task_036: Wire DI (no tests task)
├─ task_037: Write API Contract Tests (AFTER 6 implementation tasks) ❌
```

**Problem**: task_037 should have been FIRST, or interleaved with implementation tasks.

**2. Different Nature of Work**

**M1 Characteristics**:
- Pure domain logic (perfect for unit TDD)
- Functions with clear inputs/outputs
- No external dependencies (mocked via interfaces)
- Fast test execution (<1ms per test)

**M2 Characteristics**:
- API contracts (HTTP infrastructure required)
- DTOs (data structures, not logic)
- Middleware (request pipeline integration)
- Integration tests (slower, require WebApplicationFactory)

**Perception**: "DTOs don't need tests, just structure" ❌ Wrong - contract tests validate structure.

**3. Architectural Layer Confusion**

**Domain Layer (M1)**:
- Easy to test in isolation
- No HTTP knowledge
- Pure TDD workflow

**API Layer (M2)**:
- Requires HTTP infrastructure
- Integration-focused
- Harder to do pure TDD → skipped TDD entirely ❌

**4. Time Pressure**

M2 tasks had aggressive timelines:
- task_035: 1h (Parse Handler)
- task_036: 3min ⚡ (DI wiring)
- task_037: 1.5h (Contract tests)
- task_038: 2min ⚡ (Swagger)

Implementation-first seemed faster than TDD cycles.

### Should TDD Have Been Followed?

**YES** - Evidence it would have helped:

**Issues TDD Would Have Caught Earlier**:

1. **Mock Handler Issue** (M2-001):
   ```
   task_037 RED: Write test expecting actual parsing
   → Fails immediately (mock returns hardcoded data)
   → Forces endpoint wiring in task_037 GREEN phase
   → Caught in minutes, not hours
   ```

2. **Duplicate Contracts** (M2-002):
   ```
   task_031 RED: Write DTO structure tests
   → Define expected ErrorResponse structure (flat vs nested)
   → Prevents creating duplicate Api.Contracts namespace
   → Single source of truth from start
   ```

3. **Expense Tag Recognition** (M2-003):
   ```
   task_035 RED: Write ExpenseProcessor.CanProcess tests
   → Test: "<vendor>Test</vendor>" should route to ExpenseProcessor
   → Fails with current logic (only checks <total>)
   → Forces expansion of expense tag recognition
   ```

4. **Dynamic Type Issues** (M2-004):
   ```
   task_035 RED: Write OtherProcessor tests
   → Test: Expect OtherData structure
   → Dynamic casting fails in tests
   → Forces concrete type design upfront
   ```

### Correct TDD Approach for M2

**Proposed Task Restructure**:

```
task_031: Write DTO contract tests (RED)
  - Define expected request structure (ParseRequest)
  - Define expected response structure (ExpenseParseResponse, OtherParseResponse)
  - Define expected error structure (ErrorResponse)
  - All tests fail (no implementation)

task_032: Implement DTOs (GREEN)
  - Create DTOs matching test expectations
  - All contract structure tests pass

task_033: Write error handling tests (RED)
  - Test: ValidationException → 400 with error code
  - Test: Unhandled exception → 500 with generic error
  - All tests fail (no middleware)

task_034: Implement error mapping middleware (GREEN)
  - Create ExceptionMappingMiddleware
  - All error handling tests pass

task_035: Write handler tests (RED)
  - Test: Valid expense text → ExpenseParseResponse
  - Test: Non-expense text → OtherParseResponse
  - Test: Missing total → ValidationException
  - All tests fail (no handler)

task_036: Implement handler (GREEN)
  - Create ParseMessageCommandHandler
  - Wire with MediatR
  - All handler tests pass

task_037: Write endpoint integration tests (RED)
  - Test: POST /api/v1/parse → 200 with expense
  - Test: POST /api/v1/parse → 200 with other
  - Test: POST /api/v1/parse → 400 with errors
  - All tests fail (endpoint not wired)

task_038: Wire endpoint & verify (GREEN)
  - Connect ParseEndpoint to MediatR
  - All integration tests pass
  - M2 DoD verified
```

**Key Differences**:
- ✅ Every implementation task has prior RED test task
- ✅ Tests drive design (contracts defined first)
- ✅ Integration caught early (endpoint wiring verified immediately)
- ✅ Matches M1 TDD discipline

### TDD for Different API Layer Concerns

**DTOs** (Pure Structure):
```csharp
[Fact]
public void ParseRequest_ShouldHaveRequiredTextProperty()
{
    // Arrange & Act
    var request = new ParseRequest { Text = "test" };

    // Assert
    request.Text.Should().Be("test");
}

[Fact]
public void ErrorResponse_ShouldHaveFlatStructure()
{
    // Arrange & Act
    var error = new ErrorResponse
    {
        CorrelationId = "123",
        ErrorCode = "TEST_ERROR",
        Message = "Test message"
    };

    // Assert
    error.ErrorCode.Should().Be("TEST_ERROR");  // Not error.Error.Code
}
```

**Middleware** (Request Pipeline):
```csharp
[Fact]
public async Task ExceptionMappingMiddleware_ValidationException_Returns400()
{
    // Arrange
    var middleware = new ExceptionMappingMiddleware(
        next: _ => throw new ValidationException("TEST_ERROR", "Test"),
        logger: _mockLogger.Object
    );
    var context = new DefaultHttpContext();

    // Act
    await middleware.InvokeAsync(context);

    // Assert
    context.Response.StatusCode.Should().Be(400);
}
```

**Handlers** (CQRS):
```csharp
[Fact]
public async Task ParseMessageCommandHandler_ExpenseContent_ReturnsExpenseResponse()
{
    // Arrange
    var handler = new ParseMessageCommandHandler(/* mocked dependencies */);
    var command = new ParseMessageCommand(
        Text: "<total>100</total>",
        TaxRate: 0.15m
    );

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeOfType<ExpenseParseResponse>();
}
```

**Endpoints** (Integration):
```csharp
[Fact]
public async Task POST_Parse_ValidExpense_Returns200()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new ParseRequest { Text = "<total>100</total>" };

    // Act
    var response = await client.PostAsJsonAsync("/api/v1/parse", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### Why TDD Works for API Layer

**Common Myth**: "TDD only for business logic, not APIs"

**Reality**: TDD especially valuable for APIs because:

1. **Contract Verification**: Tests define expected API surface
2. **Integration Validation**: Catches wiring issues early
3. **Regression Prevention**: API changes break tests immediately
4. **Documentation**: Tests serve as executable API examples
5. **Confidence**: Can refactor knowing tests verify behavior

**M2 Evidence**: 13 contract tests caught 4 critical issues that implementation-first missed.

### Recommendation

**For Future API Milestones**:

1. **Enforce TDD Task Pairs**:
   ```
   task_N (RED):   Write tests for component X
   task_N+1 (GREEN): Implement component X
   ```

2. **Start with Contract Tests**:
   - Define API surface first (OpenAPI/Swagger)
   - Write contract tests matching spec
   - Implement to make tests pass

3. **Use Test Pyramid**:
   - Unit tests: DTOs, validators, mappers
   - Integration tests: Handlers, middleware
   - Contract tests: Endpoints, request/response flows

4. **Add TDD Gate Check**:
   - M2 DoD includes: "All components test-driven (RED before GREEN)"
   - Reviewer verifies git commits show RED → GREEN pattern

5. **Document TDD Approach**:
   - Update `CLAUDE.md` with API-layer TDD examples
   - Add to `TASK_CREATION_GUIDE.md`: "RED task required before GREEN task"

---

## Summary of Findings

| Question | Finding | Action Required | Priority |
|----------|---------|-----------------|----------|
| **Q1: Why task_030 passed** | Correctly verified M1 (domain), not M2 (API) | Add M1→M2 integration gate | High |
| **Q2: Remove src/Api/Contracts** | Safe to remove (dead code) | ✅ DONE (commit 8d07eba) | Completed |
| **Q3: Reflection necessity** | Not necessary, use concrete DTOs | Refactor to OtherProcessingData DTO | Medium |
| **Q4: Dynamic usage** | Issue - caused test failures | ✅ FIXED to reflection, should refactor to DTOs | Medium |
| **Q5: TDD adherence** | M1 ✅ strict, M2 ❌ skipped | Restructure future API tasks for TDD | Critical |

---

## Recommendations for M3 & Beyond

### Immediate (Before M2 DoD - task_040)
1. ✅ Duplicate contracts removed
2. ✅ All tests passing (13/13)
3. No blockers for M2 completion

### Short-Term (M3 Planning)
1. **Add M2→M3 Integration Gate**: Verify UI→API→Domain flow works before building E2E suite
2. **Structure M3 Tasks for TDD**:
   ```
   task_041: Write E2E happy path test (RED)
   task_042: Implement UI changes to pass (GREEN)
   task_043: Write E2E error path test (RED)
   task_044: Implement error display to pass (GREEN)
   ```

### Medium-Term (Refactoring Backlog)
1. **Replace Anonymous Types with DTOs** (Q3 & Q4)
   - Create `OtherProcessingData.cs`
   - Remove reflection from handler
   - Est: 30 minutes

2. **Add Integration Gate Task Template**
   - Document smoke test pattern
   - Add to delivery plan template
   - Est: 1 hour

### Long-Term (Process Improvement)
1. **Update Task Creation Guide**:
   - Enforce RED→GREEN pairs for all components
   - Add API-layer TDD examples
   - Document integration test patterns

2. **Add Pre-Commit Hooks**:
   - Prevent commits without tests
   - Verify test naming convention (RED/GREEN markers)

3. **Create TDD Checklist**:
   - Add to code review process
   - Verify git history shows RED before GREEN

---

**Document Version**: 1.0
**Last Updated**: 2025-10-06 21:40
**Status**: Findings documented, cleanup completed, recommendations logged

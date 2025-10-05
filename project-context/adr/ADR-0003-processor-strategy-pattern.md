# ADR-0003: Processor Strategy Pattern for Content Classification

**Status**: Accepted
**Date**: 2025-10-05
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service must handle different types of content extracted from free-form text:

1. **Expense Claims**: Extract fields, compute tax, validate required fields, normalize data
2. **Other/Unprocessed**: Content that doesn't match expense patterns (e.g., restaurant reservations)
3. **Future Content Types** (Phase 2+): Reservations, purchase orders, travel bookings, etc.

Each content type has:
- Different validation rules (e.g., expense requires `<total>`, reservation might require `<date>` + `<party_size>`)
- Different extraction logic (expense uses XML island + inline tags, reservation might use different tags)
- Different normalization requirements (expense does tax calculation, reservation might validate business hours)
- Different persistence models (different database tables/schemas)

**Key Requirements**:
- **G3 (PRD)**: "Modular processing so new content types can be added later with minimal coupling"
- **Open/Closed Principle**: System should be open for extension (new processors) but closed for modification (existing code)
- **Testability**: Each processor should be independently testable
- **Maintainability**: Adding a reservation processor shouldn't require changing expense processor code

The system needs a way to:
1. Classify incoming content (is it an expense, reservation, or other?)
2. Route content to the appropriate processor
3. Allow processors to be added/removed without modifying the router or other processors

## Decision

**Implement the Strategy Pattern for content processing.**

### Design

```
IContentProcessor (Strategy Interface)
    ├── ExpenseProcessor (Concrete Strategy)
    ├── OtherProcessor (Concrete Strategy)
    └── ReservationProcessor (Future Concrete Strategy)

ContentRouter (Context)
    - Analyzes parsed content
    - Selects appropriate IContentProcessor
    - Delegates processing to selected strategy
```

### Key Components

1. **`IContentProcessor` Interface** (Domain/Interfaces/):
   ```csharp
   public interface IContentProcessor
   {
       string ContentType { get; }
       bool CanProcess(ParsedContent content);
       Task<ProcessingResult> ProcessAsync(ParsedContent content, CancellationToken ct);
   }
   ```

2. **`ContentRouter` (Domain/Services/)**:
   - Receives parsed content (tags extracted, XML islands identified)
   - Iterates through registered `IContentProcessor` implementations
   - Calls `CanProcess()` on each until one returns `true`
   - Delegates to that processor's `ProcessAsync()`
   - Falls back to `OtherProcessor` if no match

3. **`ExpenseProcessor` (Domain/Processors/)**:
   - `CanProcess()`: Returns `true` if `<total>` tag present OR `<expense>` XML island exists
   - `ProcessAsync()`: Validates required fields, extracts expense data, computes tax, persists to `expenses` table

4. **`OtherProcessor` (Domain/Processors/)**:
   - `CanProcess()`: Always returns `true` (fallback)
   - `ProcessAsync()`: Stores raw tags in `other_payloads` table with note for future processing

### Pipeline Within Each Processor

Each processor implements an internal pipeline:
1. **Validate**: Check required fields, business rules
2. **Extract**: Pull data from tags/XML into domain objects
3. **Normalize**: Apply formatting (dates, numbers, currency)
4. **Persist**: Save to database via repository
5. **Build Response**: Create API response DTO

## Consequences

### Positive

1. **Open/Closed Principle**: Adding `ReservationProcessor` requires:
   - Create new class implementing `IContentProcessor`
   - Register in DI container
   - Zero changes to `ExpenseProcessor`, `OtherProcessor`, or `ContentRouter`

2. **Single Responsibility**: Each processor handles one content type; no "if expense... else if reservation..." spaghetti

3. **Testability**:
   - Test `ExpenseProcessor` in isolation with mock repositories
   - Test `ContentRouter` with mock processors
   - Test `CanProcess()` logic independently of `ProcessAsync()`

4. **Extensibility**: Future processors (travel, purchase orders) follow same pattern

5. **Composability**: Processors can be composed (e.g., a processor that delegates to sub-processors)

6. **Dependency Injection Friendly**: Register processors as `IEnumerable<IContentProcessor>` and inject into router

7. **Configuration-Driven**: Can enable/disable processors via `appsettings.json` without code changes

8. **Parallel Development**: Different developers can work on different processors independently

### Negative

1. **Indirection**: Following a request requires understanding router → processor → pipeline flow

2. **Overhead**: For a system with only 1 content type, this would be over-engineered (but we have 2+ types)

3. **Order Sensitivity**: If multiple processors return `true` from `CanProcess()`, order matters
   - Mitigation: Ensure `CanProcess()` logic is mutually exclusive, or use priority/ordering

4. **Fallback Ambiguity**: `OtherProcessor` as catch-all means no "unrecognized content" error
   - Mitigation: This is intentional per requirements (store unknown content for future handling)

## Alternatives Considered

### 1. Switch/Case in Single Service Class

```csharp
public async Task<Result> ProcessContent(ParsedContent content)
{
    switch (content.Type)
    {
        case "expense": return await ProcessExpense(content);
        case "reservation": return await ProcessReservation(content);
        default: return await ProcessOther(content);
    }
}
```

**Pros**:
- Simpler, fewer files
- All processing logic in one place

**Cons**:
- Violates Open/Closed: Adding reservation requires modifying this class
- Violates Single Responsibility: One class handles all content types
- Harder to test individual processors
- Methods grow large; class becomes god object

**Rejected because**: Doesn't support modular, extensible design required by G3.

### 2. Chain of Responsibility Pattern

Each processor has a reference to the next processor in the chain. If it can't handle the request, it passes to the next.

**Pros**:
- Decouples sender (router) from receivers (processors)
- Dynamic chain construction

**Cons**:
- More complex than Strategy for this use case
- Processors need to know about "next" processor (coupling)
- Harder to parallelize if needed

**Rejected because**: Strategy pattern is simpler and sufficient; no need for dynamic chain modification.

### 3. Event-Driven Architecture (Publish/Subscribe)

Router publishes `ContentParsed` event; processors subscribe.

**Pros**:
- Maximum decoupling
- Multiple processors can handle same event (e.g., logging + processing)

**Cons**:
- Complexity: need message bus, event handlers, subscription registration
- Harder to reason about flow (who's handling this event?)
- Overkill for synchronous HTTP request/response API

**Rejected because**: Asynchronous event processing not needed for Phase 1; adds unnecessary complexity.

### 4. Plugin Architecture (Load Processors from Assemblies)

Processors implemented as separate DLLs, loaded at runtime via reflection.

**Pros**:
- Ultimate extensibility (add processors without recompiling app)
- Versioning per processor

**Cons**:
- Massive complexity (assembly loading, versioning, security)
- Not needed for monolith deployment

**Rejected because**: Way beyond Phase 1 requirements; YAGNI violation.

## Implementation Guidelines

### Dependency Injection Registration

```csharp
// Program.cs
services.AddScoped<IContentProcessor, ExpenseProcessor>();
services.AddScoped<IContentProcessor, OtherProcessor>();
services.AddScoped<ContentRouter>();
```

### ContentRouter Implementation

```csharp
public class ContentRouter
{
    private readonly IEnumerable<IContentProcessor> _processors;

    public ContentRouter(IEnumerable<IContentProcessor> processors)
    {
        _processors = processors;
    }

    public async Task<ProcessingResult> RouteAsync(ParsedContent content, CancellationToken ct)
    {
        var processor = _processors.FirstOrDefault(p => p.CanProcess(content))
                        ?? _processors.First(p => p.ContentType == "other");

        return await processor.ProcessAsync(content, ct);
    }
}
```

### ExpenseProcessor CanProcess Logic

```csharp
public bool CanProcess(ParsedContent content)
{
    // Check for expense island OR inline total tag
    return content.XmlIslands.Any(x => x.Name == "expense")
        || content.InlineTags.ContainsKey("total");
}
```

### Testing Example

```csharp
[Fact]
public async Task ExpenseProcessor_WithValidExpense_ReturnsSuccessResult()
{
    // Arrange
    var mockRepo = new Mock<IExpenseRepository>();
    var processor = new ExpenseProcessor(mockRepo.Object, ...);
    var content = new ParsedContent { InlineTags = new() { ["total"] = "100.00" } };

    // Act
    var result = await processor.ProcessAsync(content, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue();
    mockRepo.Verify(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

## Future Enhancements

1. **Priority/Ordering**: Add `Priority` property to `IContentProcessor` for explicit ordering
2. **Async Processors**: Support processors that call external APIs (e.g., OCR, LLM validation)
3. **Processor Composition**: A "CompositeProcessor" that delegates to multiple sub-processors
4. **Configuration**: Enable/disable processors via `appsettings.json`:
   ```json
   "Modules": {
     "Expense": { "Enabled": true },
     "Reservation": { "Enabled": false }
   }
   ```

## References

- PRD + Technical Specification (v0.1), Section 7: Patterns (Strategy for processor selection)
- Gang of Four Design Patterns: Strategy Pattern
- Martin Fowler on Strategy: https://refactoring.guru/design-patterns/strategy
- Open/Closed Principle (SOLID): https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle

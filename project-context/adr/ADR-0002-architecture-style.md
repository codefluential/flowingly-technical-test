# ADR-0002: Architecture Style - Clean/Hexagonal with CQRS-lite

**Status**: Accepted
**Date**: 2025-10-05
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service needs an architectural style that supports:

1. **Modularity**: Ability to add new content processors (e.g., reservations) without coupling
2. **Testability**: Domain logic isolated from infrastructure concerns
3. **Maintainability**: Clear separation of concerns for easy handover
4. **Extensibility**: Accommodate future features with minimal refactoring
5. **Simplicity**: Avoid over-engineering for MVP scope; balance sophistication with delivery timeline

The service has distinct concerns:
- **Presentation**: HTTP API endpoints
- **Application Logic**: Orchestration, validation, command handling
- **Domain Logic**: Parsing, normalization, tax calculation, business rules
- **Infrastructure**: Database access, logging, external services

The test requirements emphasize demonstrating "broad coding capabilities" and understanding of architectural patterns while keeping the implementation practical and maintainable.

## Decision

**Adopt Clean/Hexagonal Architecture with a "lite" CQRS approach.**

### Architecture Layers

```
/src
  Api/            → Presentation Layer (ASP.NET Core)
  Application/    → Application Layer (Commands, Queries, Handlers, Validators)
  Domain/         → Domain Layer (Core business logic, Ports)
  Infrastructure/ → Infrastructure Layer (Adapters: EF Core, Repos, Logging)
/contracts/       → Shared DTOs (request/response contracts)
```

### Hexagonal Architecture (Ports & Adapters)

- **Ports** (interfaces): Defined in `Domain/` layer
  - `ITagValidator`, `IXmlIslandExtractor`, `IContentRouter`, `ITaxCalculator`, `INormalizer`
  - `IMessageRepository`, `IExpenseRepository`, `IProcessingLogRepository`
- **Adapters** (implementations): Implemented in `Infrastructure/`
  - `TagValidator`, `XmlIslandExtractor`, `PostgresMessageRepository`, etc.
- **Dependency Direction**: All dependencies point inward toward Domain
  - `Api` → `Application` → `Domain` ← `Infrastructure`
  - Infrastructure and Api depend on Domain, not vice versa

### CQRS-lite

- **Command/Query separation** at the handler level (not full event sourcing)
- **Commands**: `ParseMessageCommand` → `ParseMessageCommandHandler`
  - Mutates state (creates messages, expenses, logs)
  - Returns success/failure with correlation ID
- **Queries** (future): `GetExpenseByIdQuery` → `GetExpenseByIdQueryHandler`
  - Read-only operations
  - No mutations
- **No Event Sourcing** in v1 (unnecessary complexity for MVP)
- **No CQRS infrastructure** (no separate read/write stores)

### Clean Architecture Principles Applied

1. **Independence of Frameworks**: Domain logic has no ASP.NET, EF Core, or library dependencies
2. **Testability**: Domain can be tested without database, HTTP, or external services
3. **Independence of UI**: Domain doesn't know about HTTP, JSON, or API contracts
4. **Independence of Database**: Domain uses repository interfaces; swapping Postgres is possible
5. **Business Rules Isolation**: Tax calculation, validation, normalization live in Domain

## Terminology & Concepts

### Hexagonal Architecture (Ports & Adapters) in a Nutshell

The core **Domain** holds the business rules (parsing, normalization, tax logic). It talks to the outside world through **ports** (interfaces). **Adapters** implement those ports for specific technologies (e.g., EF Core repository for Postgres, HTTP controller for API endpoints). This decouples domain logic from frameworks and I/O.

**Why This Matters**:
- Business logic is independent of infrastructure (easier to test and swap implementations)
- Clear seams for **Dependency Injection (DI)** and mocking in tests
- Supports adding new processors (e.g., reservation module) without touching transport/storage details

### Parsing vs Normalization

- **Parsing**: Extract structure from raw text (e.g., find `<total>` tag and read its content as a decimal value)
- **Normalization**: Clean and standardize parsed values:
  - Strip commas, currency symbols (`$35,000.00` → `35000.00`)
  - Parse dates into ISO format (`27 April 2022` → `2022-04-27`)
  - Compute tax breakdown (total inclusive → exclusive + sales tax)
  - Format time as `HH:mm`

### CQRS-lite (Command/Query Separation)

We separate **commands** (state-changing operations like "parse & store") from **queries** (read-only operations). In v1:
- Commands run synchronously
- No split read/write models or separate databases (that's "full CQRS")
- Queries are future additions (e.g., `GetExpenseByIdQuery`)

**Why "Lite"**: We get the organizational benefits (clear separation of reads/writes) without the operational complexity of separate data stores or eventual consistency.

### Event Sourcing (Not Used in v1)

**What It Is**: Event sourcing stores an append-only log of domain events (e.g., `MessageParsed`, `ExpenseComputed`, `TaxCalculated`) and rebuilds application state by replaying those events.

**Pros**:
- Complete audit trail (know exactly what happened and when)
- Temporal queries (view state at any point in history)
- Event-driven architecture enables complex workflows

**Cons**:
- Projections complexity (rebuild read models from events)
- Replay logic and versioning overhead
- Idempotency and ordering challenges

**Why Not Used**: Our needs are met by straightforward persistence + processing logs for auditability. Event sourcing would add significant operational and cognitive overhead without commensurate benefit for Phase 1 MVP.

### Ports, Adapters & DI-Backed Implementations

- **Ports**: Interfaces the domain depends on (e.g., `IMessageRepository`, `IExpenseProcessor`, `ITaxCalculator`)
  - Defined in `Domain/Interfaces/`
  - Represent capabilities the domain needs from the outside world

- **Adapters**: Concrete technology implementations that plug into ports
  - Implemented in `Infrastructure/` (e.g., `PostgresMessageRepository`, `TaxCalculator`)
  - Or in `Api/` (e.g., HTTP controllers adapting HTTP requests to domain commands)

- **DI-Backed**: Adapters are registered in the Dependency Injection container
  - App wires them at runtime via `Program.cs`: `services.AddScoped<IMessageRepository, PostgresMessageRepository>()`
  - Tests inject fakes/mocks: `new ParseHandler(mockRepository.Object)`
  - Domain code only depends on interfaces, never concrete implementations

## Consequences

### Positive

1. **Clear Boundaries**: Each layer has well-defined responsibilities
2. **Testability**: Domain logic can be unit-tested in isolation; integration tests focus on layer boundaries
3. **Flexibility**: New processors (e.g., reservation) added without touching existing code (Open/Closed Principle)
4. **Maintainability**: New developers can understand the system by reading Domain first
5. **Dependency Inversion**: High-level policy (Domain) doesn't depend on low-level details (Infrastructure)
6. **Technology Agnostic Domain**: Could swap ASP.NET for another framework without changing business logic
7. **Explicit Contracts**: Ports (interfaces) make dependencies and responsibilities obvious
8. **Future-Ready**: CQRS-lite foundation allows evolution to full CQRS/event sourcing if needed

### Negative

1. **More Files**: More abstractions (interfaces, handlers) than simple layered architecture
2. **Indirection**: Following a request through Endpoint → Handler → Processor → Repository requires navigation
3. **Boilerplate**: Each command needs: DTO, validator, handler, potentially tests for each
4. **Learning Curve**: Developers unfamiliar with Hexagonal/Clean need onboarding
5. **Overkill Risk**: For very simple CRUD, this might be over-architected (but this app has complex parsing logic)

### Mitigation Strategies

- **Documentation**: Provide architecture diagrams and "walking skeleton" examples in README
- **ADRs**: Explain patterns (this document, ADR-0003 on Strategy pattern)
- **Code Comments**: Annotate key interfaces/classes with their role (Port vs Adapter)
- **Folder Structure**: Clear naming makes layer boundaries obvious
- **Tests**: Demonstrate testing patterns for each layer

## Alternatives Considered

### 1. Simple Layered Architecture (Controller → Service → Repository)

**Pros**:
- Fewer files, less abstraction
- Familiar to most developers
- Faster initial development

**Cons**:
- Services often become "god classes" with mixed concerns
- Harder to test (controllers depend on concrete services)
- Business logic leaks into controllers or repositories
- Adding new processors means modifying existing service classes

**Rejected because**: Doesn't meet the requirement to "demonstrate broad coding capabilities" with architectural patterns.

### 2. Full CQRS with Event Sourcing

**Pros**:
- Complete separation of read/write models
- Full audit trail via event log
- Temporal queries (state at any point in time)
- Scales to high-throughput systems

**Cons**:
- Massive complexity for MVP
- Requires event store (additional infrastructure)
- Steep learning curve
- Over-engineered for simple text parsing

**Rejected because**: Violates YAGNI principle for Phase 1; CQRS-lite provides 80% of benefits with 20% of complexity.

### 3. Vertical Slice Architecture

**Pros**:
- Feature-centric folders (everything for "ParseExpense" in one place)
- Minimal coupling between features
- Easy to understand feature scope

**Cons**:
- Shared domain logic (e.g., tax calculator) harder to organize
- Less emphasis on layering (which is an educational goal here)
- Harder to showcase traditional architectural patterns

**Rejected because**: While modern and pragmatic, doesn't demonstrate understanding of classic patterns like Hexagonal Architecture.

### 4. Microservices (Separate services for Expense Processor, Reservation Processor, etc.)

**Pros**:
- Maximum isolation between processors
- Independent scaling and deployment

**Cons**:
- Extreme overkill for MVP
- Deployment complexity (orchestration, service discovery)
- Network overhead, distributed transactions
- Not suitable for Render free tier

**Rejected because**: Violates simplicity principle; single monolith is appropriate for Phase 1.

## Implementation Guidelines

### Dependency Injection Registration

```csharp
// Program.cs or Startup.cs
services.AddScoped<ITagValidator, TagValidator>();
services.AddScoped<IContentRouter, ContentRouter>();
services.AddScoped<IExpenseProcessor, ExpenseProcessor>();
services.AddScoped<IMessageRepository, PostgresMessageRepository>();
services.AddMediatR(Assembly.GetExecutingAssembly()); // For CQRS handlers
```

### Domain Port Example

```csharp
// Domain/Interfaces/ITaxCalculator.cs
public interface ITaxCalculator
{
    (decimal ExcludingTax, decimal SalesTax) CalculateFromInclusive(decimal inclusiveTotal, decimal taxRate);
}
```

### Infrastructure Adapter Example

```csharp
// Infrastructure/Services/TaxCalculator.cs
public class TaxCalculator : ITaxCalculator
{
    public (decimal ExcludingTax, decimal SalesTax) CalculateFromInclusive(decimal inclusiveTotal, decimal taxRate)
    {
        var excludingTax = inclusiveTotal / (1 + taxRate);
        var salesTax = inclusiveTotal - excludingTax;
        return (Math.Round(excludingTax, 2), Math.Round(salesTax, 2));
    }
}
```

### CQRS Command Example

```csharp
// Application/Commands/ParseMessageCommand.cs
public record ParseMessageCommand(string Text, decimal? TaxRate, string? Currency);

// Application/Handlers/ParseMessageCommandHandler.cs
public class ParseMessageCommandHandler : IRequestHandler<ParseMessageCommand, ParseMessageResult>
{
    private readonly IContentRouter _router;
    // ... dependencies injected

    public async Task<ParseMessageResult> Handle(ParseMessageCommand request, CancellationToken ct)
    {
        // Orchestration logic
    }
}
```

## References

- PRD + Technical Specification (v0.1), Section 3: High-Level Solution Architecture
- PRD Section 7: Backend Design & Implementation (Layering, Patterns)
- Clean Architecture (Robert C. Martin): https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- Hexagonal Architecture (Alistair Cockburn): https://alistair.cockburn.us/hexagonal-architecture/
- CQRS Pattern: https://martinfowler.com/bliki/CQRS.html

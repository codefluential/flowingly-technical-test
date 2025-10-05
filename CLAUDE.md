# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Flowingly Parsing Service** — A modular text-ingestion and parsing service that extracts structured data from free-form text containing inline tags and XML islands, starting with Expense Claims processing.

### Core Objective
Accept raw text (e.g., email bodies), validate tag integrity, extract expense data, compute NZ GST tax breakdowns (from inclusive totals), and return normalized JSON. Non-expense content is stored as "Other/Unprocessed" for future processors.

## Architecture

**Style**: Clean/Hexagonal Architecture with CQRS-lite approach

### Backend Structure (.NET 8)
```
/src
  Api/            → Endpoints, DI registration, Swagger, Middleware
  Application/    → Commands/Queries, Validators, Handlers (CQRS)
  Domain/         → Parsers, Value Objects, Calculators, Interfaces (Ports)
  Infrastructure/ → EF Core, Repositories (Adapters), Logging, Config
/contracts/       → Request/Response DTOs shared with tests
```

### Frontend Structure (React + Vite + TypeScript)
```
/client
  /src
    /components   → UI components
    /api          → API client/fetch wrappers
    /types        → TypeScript interfaces
```

### Key Design Patterns
- **Strategy Pattern**: Processor selection (ExpenseProcessor vs OtherProcessor) based on content classification
- **Pipeline Pattern**: Within each processor (Validate → Extract → Normalize → Persist → BuildResponse)
- **Repository Pattern**: Data persistence abstraction
- **Ports & Adapters**: All external dependencies (DB, parsers) behind interfaces

### Critical Components
- `ITagValidator`: Stack-based tag integrity validation (reject unclosed tags)
- `IXmlIslandExtractor`: Secure XML parsing (DTD/XXE disabled)
- `IContentRouter`: Routes content to appropriate processor
- `ITaxCalculator`: Computes GST (tax-inclusive to exclusive conversion)
- `INormalizer`: Number/date/time normalization

## Development Methodology

### TDD/BDD Approach
- **Prioritize happy path first**, then core edge cases
- Avoid over-engineering; keep tests reasonable
- Use xUnit + FluentAssertions (.NET), Playwright (React E2E)

### Code Principles
- Follow SOLID, DRY, KISS, YAGNI
- Simple, maintainable, extensible over complex
- Verify against specification before implementing
- Ensure type safety: align database schema, application types, and TypeScript interfaces

### Evidence-Based Decisions
- Check implementation against specification documents
- Read existing tests to understand application behavior
- Verify what has been implemented vs. what needs implementation

## Key Configuration

### Tax & Currency Defaults
- Default tax rate: **0.15** (NZ GST)
- Default currency: **NZD**

### Validation Rules
- `<total>` (tax-inclusive) is **required** for expenses; reject if missing
- `<cost_centre>` is **optional**; default to `UNKNOWN` if absent
- Any unclosed tag → **reject with 400 error**

### API Design
- Single endpoint: `POST /api/v1/parse`
- URI-based versioning (`/api/v1/`)
- Response includes correlation ID for traceability

## Documentation Standards

### Maintain Minimal, Purposeful Documentation
Balance between markdown guides and code comments—choose the most maintainable option for each context.

### Required Documentation
1. **ADRs** at `project-context/adr/`
   - ADR-0001: Storage choice (Postgres only)
   - ADR-0002: Architecture style (Clean/Hexagonal + CQRS-lite)
   - ADR-0003: Processor Strategy pattern
   - ADR-0004: Swagger for API contract
   - ADR-0005: Versioning via URI

2. **Build Log** at `project-context/build-logs/BUILDLOG.md`
   - Date, changes made, rationale, issues, testing notes

3. **Specification Documents** at `project-context/specifications/`
   - Primary: `prd-technical_spec.md` (v0.1)   
   - Requirements: `project-context/requirements-and-analysis/Full Stack Engineer Test (Sen) V2.pdf`

**These are the key documents** — always reference these before planning or implementing tasks.

## Git Conventions

- Use **conventional commits** syntax: `<type>[scope]: <description>`
- Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `perf`, `style`
- Include co-authorship footer when using Claude Code:
  ```
  🤖 Generated with [Claude Code](https://claude.com/claude-code)

  Co-Authored-By: Claude <noreply@anthropic.com>
  ```

## Technology Stack

### Backend
- .NET 8 (Minimal API or Controllers)
- xUnit, FluentAssertions, FluentValidation
- Serilog (structured logging with correlation IDs)
- EF Core with Postgres
- Swashbuckle (Swagger/OpenAPI)

### Frontend
- React + Vite + TypeScript
- Playwright for E2E testing

### Deployment
- **Platform**: Render (free tier)
- **Database**: Render Postgres (free tier)
- **Environments**: Local dev + Render prod (main branch)
- **CI/CD**: GitHub Actions with Render integration

## Specification Scope

This is a **Phase 1, minimal viable product** demonstrating:
- End-to-end SDLC/DevOps understanding
- Clean architecture and cross-cutting concerns
- Practical simplicity balanced with broad technical thinking

**Out of Scope**: Phase 2 features (reservation processing), advanced auth, complex reporting.

## Key Verification Points

Before implementing:
1. Check specification: `project-context/specifications/prd-technical_spec.md`
2. Review clarifications: `project-context/specifications/prd-tech-sepec-v1-review.md`
3. Verify existing tests to understand current behavior
4. Ensure alignment: DB schema ↔ domain models ↔ DTOs ↔ TypeScript types

When completing work:
1. Update `BUILDLOG.md` with changes, rationale, testing notes
2. Create/update ADRs for architectural decisions
3. Run tests and verify type safety end-to-end

## Framework-Specific Guidelines

### React-Specific Rules
- Use Vite for fast development and optimized builds
- Leverage TypeScript strict mode for type safety
- Implement React Query or simple fetch wrappers for API state management
- Follow component-based architecture with clear separation of concerns
- Use Playwright for E2E testing with user-centric test scenarios
- Ensure accessibility (ARIA labels, keyboard navigation, focus management)

### .NET Core API-Specific Rules
- Use .NET 8 with Minimal APIs or Controllers (prefer Minimal APIs for simplicity)
- Implement FluentValidation for request validation
- Use FluentAssertions in xUnit tests for readable test assertions
- Configure Serilog for structured logging with correlation IDs
- Apply EF Core migrations for database schema changes
- Implement Ports (interfaces) in Domain, Adapters (implementations) in Infrastructure
- Register all dependencies via DI in Program.cs
- Use async/await throughout for I/O operations
- Expose Swagger UI at `/swagger` in development environment

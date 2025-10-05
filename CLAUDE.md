# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Flowingly Parsing Service** — A modular text-ingestion and parsing service that extracts structured data from free-form text containing inline tags and XML islands, starting with Expense Claims processing.

### Core Objective
Accept raw text (e.g., email bodies), validate tag integrity, extract expense data, compute NZ GST tax breakdowns (from inclusive totals), and return normalized JSON. Non-expense content is stored as "Other/Unprocessed" for future processors.

### Current Project State
**Phase**: Specification and planning complete, **ready for implementation**.
- All ADRs documented (10 total)
- PRD v0.3 finalized with external review incorporated
- Delivery plan with milestone breakdown completed
- Task decomposition complete (50 tasks across M0-M3)
- Progress tracking system deployed (3-file integrated system)
- 20 agents copied to `.claude/agents/` for task execution
- No code implementation exists yet (no .NET solution, no React app)

Refer to `project-context/build-logs/BUILDLOG.md` for detailed progress history and `project-context/implementation/PROGRESS.md` for current status.

## Common Development Commands

### Backend (.NET 8)
```bash
# Build and run (once solution is created)
dotnet build
dotnet run --project src/Api

# Run tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"

# Database migrations (EF Core)
dotnet ef migrations add MigrationName --project src/Infrastructure
dotnet ef database update --project src/Api
```

### Frontend (React + Vite)
```bash
# Install dependencies
npm install

# Run dev server
npm run dev

# Build for production
npm run build

# Run E2E tests (Playwright)
npm run test:e2e

# Run specific E2E test
npx playwright test path/to/test.spec.ts
```

**Note**: These commands will be available once project scaffolding is complete. Currently no implementation exists.

### Progress Tracking
```bash
# Start a task (updates tasks.json, PROGRESS.md)
./scripts/update-progress.sh task_001 in_progress

# Complete a task
./scripts/update-progress.sh task_001 completed

# Complete with test counts
./scripts/update-progress.sh task_014 completed unit 7

# Block a task
./scripts/update-progress.sh task_015 blocked

# Query current status
jq '.progress_tracking' project-context/implementation/tasks/tasks.json

# Check milestone progress
jq '.progress_tracking.milestones_completed' project-context/implementation/tasks/tasks.json

# View progress dashboard
cat project-context/implementation/PROGRESS.md
```

**Workflow**: Use `update-progress.sh` before/after each task. Script automatically:
- Updates `tasks.json` with status and timestamps
- Updates `PROGRESS.md` checkboxes and metrics
- Appends to `BUILDLOG.md` for milestone gates (task_010, 030, 040, 050)
- Suggests commit messages

See `project-context/implementation/TRACKING-WORKFLOW.md` for complete workflow documentation.

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

## Implementation Workflow

### Task-Based Execution System
The project uses a **50-task decomposition system** with 4 milestone gates (M0, M1, M2, M3):

**Milestone Structure**:
- **M0: Minimal Scaffold** (10 tasks, 4 hours) - Solution structure, echo flow, README
- **M1: Core Parsing & Validation** (20 tasks, 1 day) - TDD cycles for parsers, 30+ unit tests
- **M2: API Contracts** (10 tasks, 4 hours) - DTOs, validation, error handling, 10+ contract tests
- **M3: UI & E2E Tests** (10 tasks, 4 hours) - Enhanced UI, 5+ E2E tests, **SUBMITTABLE**

**Task Execution Flow**:
1. Check `project-context/implementation/tasks/tasks.json` for task dependencies
2. Read individual task file (e.g., `tasks/task_001.json`) for full context
3. Mark task in-progress: `./scripts/update-progress.sh task_001 in_progress`
4. Execute task following acceptance criteria
5. Mark completed: `./scripts/update-progress.sh task_001 completed [test_type] [count]`
6. Commit implementation, then commit progress files separately
7. Move to next task in sequence

**Parallel Execution Groups**: 15+ tasks can run concurrently (defined in tasks.json). Respect dependencies.

**Milestone Gates (DoD Tasks)**: task_010, task_030, task_040, task_050 verify milestone completion and auto-update BUILDLOG.

### TDD Workflow
**M1 uses strict TDD cycles** (RED → GREEN → REFACTOR):
- Tag Validation: task_014 (RED) → task_015 (GREEN) → task_016 (REFACTOR)
- Number Normalization: task_017 (RED) → task_018 (GREEN)
- Banker's Rounding: task_019 (RED) → task_020 (GREEN)
- Tax Calculator: task_021 (RED) → task_022 (GREEN)
- Time Parser: task_023 (RED) → task_024 (GREEN)
- XML Extractor: task_025 (RED) → task_026 (GREEN)
- Content Router: task_027 (RED) → task_028 (GREEN)
- Expense Processor: task_029 (RED) → task_030 (GREEN + M1 DoD)

**Test Coverage Targets** (ADR-0010):
- Unit tests: 30+ (M1)
- Contract tests: 10+ (M2)
- E2E tests: 5+ (M3)
- **Total: 45+ tests** for submission

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
1. **ADRs** at `project-context/adr/` (10 total)
   - ADR-0001: Storage choice (SQLite local dev, Postgres production)
   - ADR-0002: Architecture style (Clean/Hexagonal + CQRS-lite)
   - ADR-0003: Processor Strategy pattern
   - ADR-0004: Swagger for API contract
   - ADR-0005: Versioning via URI
   - ADR-0006: API key authentication
   - ADR-0007: Response Contract Design (expense XOR other)
   - ADR-0008: Parsing and Validation Rules (stack-based tag validation)
   - ADR-0009: Tax Calculation with Banker's Rounding (MidpointRounding.ToEven)
   - ADR-0010: Test Strategy and Coverage (45+ tests: unit, contract, E2E)

2. **Build Log** at `project-context/build-logs/BUILDLOG.md`
   - Chronological order (oldest→newest), append new entries to end
   - Date, changes made, rationale, issues, testing notes

3. **Specification Documents** at `project-context/specifications/`
   - Primary: `prd-technical_spec.md` (v0.3 - current)
   - Requirements: `project-context/requirements-and-analysis/Full Stack Engineer Test (Sen) V2.pdf`

4. **Implementation System** at `project-context/implementation/`
   - `tasks/tasks.json`: 50-task orchestration with dependencies
   - `PROGRESS.md`: Real-time progress dashboard
   - `TRACKING-WORKFLOW.md`: Tracking system documentation
   - Individual task files: Self-contained context for agent execution

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
1. Check specification: `project-context/specifications/prd-technical_spec.md` (v0.3)
2. Review existing tests to understand current behavior
3. Check BUILDLOG.md for implementation history and context
4. Ensure alignment: DB schema ↔ domain models ↔ DTOs ↔ TypeScript types

When completing work:
1. Update progress: `./scripts/update-progress.sh <task_id> completed`
2. Update `BUILDLOG.md` with changes, rationale, testing notes
3. Create/update ADRs for architectural decisions
4. Run tests and verify type safety end-to-end
5. Commit implementation and progress files separately

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

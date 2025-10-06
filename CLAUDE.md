# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Flowingly Parsing Service** — A modular text-ingestion and parsing service that extracts structured data from free-form text containing inline tags and XML islands, starting with Expense Claims processing.

### Core Objective
Accept raw text (e.g., email bodies), validate tag integrity, extract expense data, compute NZ GST tax breakdowns (from inclusive totals), and return normalized JSON. Non-expense content is stored as "Other/Unprocessed" for future processors.

### Current Project State
**Phase**: M1 Complete → Ready for M2 API Contracts (30/50 tasks, 60% complete)
- ✅ M0 Minimal Scaffold Complete (10/10 tasks)
- ✅ M1 Core Parsing & Validation Complete (20/20 tasks, 18 unit tests passing)
- 📋 M2 API Contracts (0/10 tasks) - Next
- 📋 M3 UI & E2E Tests (0/10 tasks) - Planned
- All ADRs documented (10 total)
- PRD v0.3 finalized with external review incorporated
- Task decomposition complete (50 tasks across M0-M3)
- Progress tracking system deployed (3-file integrated system)
- 20 agents copied to `.claude/agents/` for task execution

**Implemented Components**:
- .NET 8 solution with Clean Architecture layers (Api, Application, Domain, Infrastructure)
- React + Vite + TypeScript frontend with working echo flow
- **Tag validator** with stack-based validation
- **XML island extractor** with DTD/XXE protection
- **Tax calculator** with Banker's Rounding (MidpointRounding.ToEven)
- **Number normalizer** (handles commas, decimals)
- **Time parser** for time expressions
- **Content router** for processor selection
- **Expense processor** with full parsing pipeline (Validate → Extract → Normalize → Persist → BuildResponse)
- **18 unit tests** passing (30+ target for M1)

Refer to `project-context/build-logs/BUILDLOG.md` for detailed progress history and `project-context/implementation/PROGRESS.md` for current status.

## Environment Setup

### .NET 8 Path Configuration
**CRITICAL**: dotnet is installed at `/home/adarsh/.dotnet/dotnet` (version 8.0.414) but requires PATH configuration.

**For all bash commands using dotnet**, prefix with:
```bash
export PATH="$HOME/.dotnet:$PATH"
```

**Example**:
```bash
export PATH="$HOME/.dotnet:$PATH" && dotnet build
```

**Full Installation Path**: `/home/adarsh/.dotnet/dotnet`
**Version**: 8.0.414
**DOTNET_ROOT**: `$HOME/.dotnet`

## Quick Start (Running the Application)

### 1. Backend API
```bash
# From project root
export PATH="$HOME/.dotnet:$PATH"
dotnet run --project src/Api
# API runs on http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

### 2. Frontend (in new terminal)
```bash
# From project root
cd client
npm install  # First time only
npm run dev
# Frontend runs on http://localhost:5173
```

### 3. Verify
1. Open http://localhost:5173 in browser
2. Paste sample expense text in textarea
3. Click "Parse" button
4. Verify parsed expense data appears with correlation ID

**Sample Test Input**:
```
Hi Yvaine, Please create an expense claim for the below. Relevant details are:
<expense><cost_centre>DEV002</cost_centre><total>1024.01</total><payment_method>personal card</payment_method></expense>
```

## Common Development Commands

### Backend (.NET 8)
```bash
# IMPORTANT: Always export PATH first
export PATH="$HOME/.dotnet:$PATH"

# Build and run (once solution is created)
dotnet build
dotnet run --project src/Api

# Run tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"

# Run tests by category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Contract"

# Database migrations (EF Core)
dotnet ef migrations add MigrationName --project src/Infrastructure
dotnet ef database update --project src/Api
```

### Frontend (React + Vite)
```bash
# Navigate to client directory
cd client

# Install dependencies
npm install

# Run dev server (http://localhost:5173)
npm run dev

# Build for production
npm run build

# Run E2E tests (Playwright) - requires both servers running
npm run test:e2e

# Run specific E2E test
npx playwright test path/to/test.spec.ts
```

### MCP Servers & Tools

This project uses **Model Context Protocol (MCP) servers** for enhanced development capabilities:

#### Serena MCP (Semantic Code Analysis)
**Status**: ✅ Configured and indexed (as of M0 completion)
**Purpose**: Semantic code navigation, refactoring, and symbol-based editing for C# codebase

**When to Use**:
- ✅ Exploring code structure (classes, methods, interfaces)
- ✅ Finding all references to a symbol across the project
- ✅ Refactoring (renaming, changing signatures)
- ✅ Editing code by symbol (not line numbers)
- ✅ Understanding dependencies and call chains

**Key Tools**:
```bash
# Symbol exploration
Get symbols overview for src/Domain/Services/TaxCalculator.cs
Find symbol "TaxCalculator/CalculateFromInclusive" with include_body=true
Find all references to "ITaxCalculator"

# Code editing
Replace symbol body "ExpenseProcessor/ProcessAsync" with [new implementation]
Insert after symbol "TaxCalculator/CalculateFromInclusive" [new method]

# Project memory
Write a memory "architecture-notes" with [content]
List all memories
```

**Setup**:
- Project indexed at `.serena/cache/` (gitignored)
- Configuration in `.serena/project.yml`
- Language: C#, 20+ tools available
- See `project-context/learnings/serena-mcp-guide.md` for comprehensive guide

**Re-indexing** (for updated code symbols):
- **Automatic**: Re-indexes after EVERY completed task (runs in background, non-blocking)
- **Manual**: Run `./scripts/reindex-serena.sh` for immediate foreground re-indexing
- **Trigger**: `update-progress.sh <task_id> completed` auto-starts background re-indexing
- **Performance**: Background process doesn't slow down workflow

**Best Practices**:
- Start with `get_symbols_overview` before reading full files
- Use `find_symbol` with `depth=1` to see class methods
- Find references before refactoring
- Prefer symbol editing over line-based editing

---

#### Context7 MCP (Library Documentation)
**Purpose**: Fetch up-to-date documentation for libraries and frameworks

**When to Use**:
- Looking up .NET, React, or other library APIs
- Need current documentation (beyond Claude's knowledge cutoff)
- Understanding library-specific patterns

**Usage**:
```
Get documentation for Entity Framework Core migrations
Get docs for React Query useQuery hook
```

---

#### Playwright MCP (Browser Automation & Testing)
**Purpose**: Browser automation for E2E testing and web interaction

**When to Use**:
- ✅ Writing/debugging E2E tests (M3 milestone)
- ✅ Testing UI flows in the browser
- ✅ Verifying form submissions, navigation, API responses
- ✅ Taking screenshots for debugging
- ✅ Inspecting console errors and network requests

**Key Tools**:
```bash
# Navigation
Navigate to http://localhost:3000
Take screenshot of current page

# Interaction
Click on element (e.g., submit button)
Type text into input field
Fill form with multiple fields

# Verification
Get page snapshot (accessibility tree)
Get console messages
Get network requests
Wait for text to appear/disappear

# Session management
List/create/close/select browser tabs
```

**Usage Example (E2E Test Flow)**:
```
1. Navigate to http://localhost:5173
2. Take snapshot to verify page structure
3. Type sample expense text into textarea
4. Click "Parse" button
5. Wait for response
6. Verify parsed expense data appears
7. Take screenshot of results
```

**Best for M3**: Writing the 5+ E2E tests required for submission.

---

#### IDE MCP (VS Code Integration)
**Purpose**: Language diagnostics and code execution

**Tools**:
- `getDiagnostics`: Get compiler errors/warnings from VS Code
- `executeCode`: Run code in Jupyter kernels (for notebooks)

---

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
- **Tracks task duration** (started_at, completed_at, duration_minutes calculated automatically)
- Updates `PROGRESS.md` checkboxes and metrics
- Appends to `BUILDLOG.md` for milestone gates (task_010, 030, 040, 050)
- Triggers background Serena re-indexing for semantic code analysis
- Suggests commit messages

**Duration Tracking**:
- `started_at`: ISO timestamp when task marked in_progress
- `completed_at`: ISO timestamp when task marked completed
- `duration_minutes`: Auto-calculated on completion (rounded to nearest minute)
- Query duration metrics: `jq '.tasks[] | select(.duration_minutes != null) | {id, name, duration_minutes}' project-context/implementation/tasks/tasks.json`

See `project-context/implementation/TRACKING-WORKFLOW.md` for complete workflow documentation.

### Slash Commands (AADF-Compliant Git Workflow)

This project includes custom slash commands for AADF-compliant commits:

```bash
# Execute prompts from a file (for complex multi-step tasks)
/aadf-prompt-file path/to/instructions.md

# Create AADF-compliant commit from all changes (auto-stages everything)
/aadf-commit "description of changes made"

# Create AADF-compliant commit from staged files only
/aadf-commit-staged "description of staged changes"
```

**When to Use**:
- `/aadf-commit`: After completing a task with multiple file changes
- `/aadf-commit-staged`: When you want fine-grained control over what's committed
- Both commands use conventional commits syntax and include Claude Code co-authorship footer

**Example**:
```bash
# After implementing tax calculator
/aadf-commit "implement ITaxCalculator with Banker's Rounding"
```

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

### Critical Components (Implemented)

**Domain Layer** (`src/Domain/`):
- `ITagValidator` / `TagValidator`: Stack-based tag integrity validation (rejects unclosed tags)
- `IXmlIslandExtractor` / `XmlIslandExtractor`: Secure XML parsing (DTD/XXE disabled)
- `IContentRouter` / `ContentRouter`: Routes content to appropriate processor
- `ITaxCalculator` / `TaxCalculator`: Computes GST (tax-inclusive to exclusive conversion)
- `NumberNormalizer`: Normalizes numbers (handles commas, decimals)
- `ITimeParser` / `TimeParser`: Parses time expressions
- `IRoundingHelper` / `RoundingHelper`: Banker's Rounding (MidpointRounding.ToEven)
- `IContentProcessor` / `ExpenseProcessor`: Complete expense parsing pipeline

**Models** (`src/Domain/Models/`):
- `Expense`: Domain entity for expense data
- `ParsedContent`: Result of content parsing
- `TaxCalculationResult`: GST calculation output
- `XmlIsland`: Extracted XML data structure
- `ProcessingResult`: Generic processing result wrapper

**Exceptions** (`src/Domain/Exceptions/`):
- `ValidationException`: Custom exception for validation failures

**API Layer** (`src/Api/`):
- `ParseEndpoint`: Single POST endpoint at `/api/v1/parse`
- `Program.cs`: DI registration, Swagger configuration, middleware setup

## Implementation Workflow

### Task-Based Execution System
The project uses a **50-task decomposition system** with 4 milestone gates (M0, M1, M2, M3):

**Milestone Structure**:
- **M0: Minimal Scaffold** (10 tasks, 4 hours) - Solution structure, echo flow, README
- **M1: Core Parsing & Validation** (20 tasks, 1 day) - TDD cycles for parsers, 30+ unit tests
- **M2: API Contracts** (10 tasks, 4 hours) - DTOs, validation, error handling, 10+ contract tests
- **M3: UI & E2E Tests** (10 tasks, 4 hours) - Enhanced UI, 5+ E2E tests, **SUBMITTABLE**

**Task Execution Flow** (Just-In-Time Task Creation):
1. **Check task file exists**: Look for `project-context/implementation/tasks/task_XXX.json`. Also check `project-context/implementation/tasks/tasks.json` for task dependencies.
2. **If task file missing**:
   - Extract metadata from `tasks.json`
   - Create task file using `TASK_CREATION_GUIDE.md` (5-10 min)
   - Validate against quality checklist
   - Commit task file: `git add tasks/task_XXX.json && git commit`
3. **If task file exists**: Read for full context (PRD sections, ADRs, deliverables, acceptance criteria)
4. **⚠️ MANDATORY: Discuss with user BEFORE execution**:
   - Present task summary (ID, name, agent, deliverables, duration)
   - Verify agent selection is appropriate for task type
   - Confirm approach and get explicit approval to proceed
   - If parallel tasks, confirm parallel execution is acceptable
   - **NEVER skip this step - always wait for user approval**
5. Mark task in-progress: `./scripts/update-progress.sh task_XXX in_progress`
6. Execute task following acceptance criteria (use specialized agent if assigned)
7. Mark completed: `./scripts/update-progress.sh task_XXX completed [test_type] [count]`
8. Commit implementation, then commit progress files separately
9. Move to next task in sequence

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
   - `tasks/TASK_CREATION_GUIDE.md`: **Comprehensive guide for creating task files**
   - Individual task files: Self-contained context for agent execution

5. **Task Creation Guide** at `project-context/implementation/tasks/TASK_CREATION_GUIDE.md`
   - **Complete methodology** for creating self-contained task files
   - **Source documents catalog**: PRD, ADRs, test brief, delivery plan
   - **Field-by-field guide**: Every JSON field explained with examples
   - **Task type patterns**: M0 scaffold, M1 TDD (RED/GREEN/REFACTOR), M2 API, M3 UI/E2E
   - **Step-by-step creation process**: From extracting metadata to validation
   - **Quality checklist**: Ensure context completeness, traceability, actionability
   - **Use this guide** when creating new task files (46 remaining tasks)

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

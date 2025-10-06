# Flowingly Parsing Service

A modular text-ingestion and parsing service that extracts structured data from free-form text containing inline tags and XML islands, starting with Expense Claims processing.

**Current Status**: M0 Minimal Scaffold Complete — Ready for M1 Core Parsing Implementation

## Quick Start

### Prerequisites
- .NET 8 SDK ([Download](https://dotnet.microsoft.com/download))
- Node.js 18+ ([Download](https://nodejs.org/))
- Git

**Important**: If dotnet is not in your PATH, add this to your `~/.bashrc`:
```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH="$DOTNET_ROOT:$PATH"
```

### 1. Clone and Setup
```bash
git clone <repository-url>
cd flowingly-technical-test
```

### 2. Run Backend API
```bash
# Ensure dotnet is in PATH (if needed)
export PATH="$HOME/.dotnet:$PATH"

# Build and run .NET API (runs on http://localhost:5000)
dotnet run --project src/Api
```

### 3. Run Frontend (in new terminal)
```bash
cd client
npm install
npm run dev  # Runs on http://localhost:5173
```

### Verify
1. Open http://localhost:5173 in browser
2. Paste sample text in textarea
3. Click "Parse" - should see mock expense result
4. Verify correlation ID in footer

✅ **Setup complete in under 5 minutes!**

---

## What This Service Does

**Core Objective**: Accept raw text (e.g., email bodies), validate tag integrity, extract expense data, compute NZ GST tax breakdowns (from inclusive totals), and return normalized JSON.

**Current Implementation** (M0 - Echo Flow):
- Backend API echoes parsed request with mock expense data
- Frontend displays mock response with correlation ID
- Swagger documentation at http://localhost:5000/swagger

**Upcoming** (M1+ Full Parsing):
- Tag validation (stack-based, reject unclosed tags)
- XML island extraction (secure parsing, DTD/XXE disabled)
- Expense data extraction with GST calculation
- Number/date/time normalization
- Content routing (Expense vs Other/Unprocessed)

---

## Architecture

**Style**: Clean/Hexagonal Architecture with CQRS-lite

```
/src
  Api/            → Endpoints, DI, Swagger, Middleware
  Application/    → Commands/Queries, Validators, Handlers
  Domain/         → Parsers, Value Objects, Interfaces (Ports)
  Infrastructure/ → EF Core, Repositories (Adapters)
/client           → React + Vite + TypeScript frontend
/contracts        → Shared DTOs (Request/Response models)
```

**Key Principles**:
- Dependency flow: Api → Application → Domain (← Infrastructure)
- Domain has zero external dependencies
- Interfaces (Ports) defined in Domain, implemented in Infrastructure (Adapters)
- Strategy Pattern for processor selection (Expense vs Other)
- Pipeline Pattern within processors (Validate → Extract → Normalize → Persist → BuildResponse)

See [ADR-0002](/project-context/adr/ADR-0002-architecture-style.md) for full architectural rationale.

---

## Development Workflow

### Backend Commands
```bash
# Ensure dotnet is in PATH
export PATH="$HOME/.dotnet:$PATH"

# Build solution
dotnet build

# Run API with hot reload
dotnet watch --project src/Api

# Run tests
dotnet test

# Run specific test category
dotnet test --filter Category=Unit
dotnet test --filter Category=Contract

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"

# Database migrations (when EF Core is configured)
dotnet ef migrations add MigrationName --project src/Infrastructure
dotnet ef database update --project src/Api
```

### Frontend Commands
```bash
cd client

# Install dependencies
npm install

# Run dev server with hot reload
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run E2E tests (requires both servers running)
npm run test:e2e

# Run specific E2E test
npx playwright test path/to/test.spec.ts
```

---

## Testing

**Coverage Targets** ([ADR-0010](/project-context/adr/ADR-0010-test-strategy-coverage.md)):
- 30+ unit tests (Domain/Application logic)
- 10+ contract tests (API integration)
- 5+ E2E tests (Playwright browser tests)
- **Total: 45+ tests** for submission

### Run Tests

**Backend Unit Tests**:
```bash
dotnet test --filter Category=Unit
```

**Backend Contract Tests**:
```bash
dotnet test --filter Category=Contract
```

**Frontend E2E Tests** (requires both servers running):
```bash
cd client
npm run test:e2e
```

**All Tests**:
```bash
dotnet test  # Backend
cd client && npm run test:e2e  # Frontend
```

### Test-Driven Development (TDD)
M1 milestone uses strict TDD cycles (RED → GREEN → REFACTOR):
- Write failing test first (RED)
- Implement minimal code to pass (GREEN)
- Refactor for quality (REFACTOR)

Example TDD pairs:
- Tag Validation: task_014 (RED) → task_015 (GREEN) → task_016 (REFACTOR)
- Tax Calculator: task_021 (RED) → task_022 (GREEN)
- XML Extractor: task_025 (RED) → task_026 (GREEN)

---

## API Documentation

### Swagger UI
When the API is running, explore interactive documentation at:
**http://localhost:5000/swagger**

### Endpoints

**POST /api/v1/parse**
- Accepts raw text with inline tags and XML islands
- Returns parsed expense data OR other/unprocessed content
- Includes correlation ID for traceability

**Request Example**:
```json
{
  "rawText": "Hi Yvaine, Please create an expense claim for the below. Relevant details are: <expense><cost_centre>DEV002</cost_centre><total>1024.01</total><payment_method>personal card</payment_method></expense> From: Ivan Jensen Sent: Friday, 16 February 2018 10:32 AM To: Antoine Lloyd <Antoine.Lloyd@example.com> Subject: test Hi Antoine, Please create a reservation..."
}
```

**Response Example (M0 Echo)**:
```json
{
  "correlationId": "abc123",
  "expense": {
    "costCentre": "UNKNOWN",
    "total": 0.0,
    "totalExcludingGst": 0.0,
    "gst": 0.0,
    "paymentMethod": "unknown"
  },
  "other": null
}
```

**Response Example (M1+ Full Parsing)**:
```json
{
  "correlationId": "abc123",
  "expense": {
    "costCentre": "DEV002",
    "total": 1024.01,
    "totalExcludingGst": 890.44,
    "gst": 133.57,
    "paymentMethod": "personal card"
  },
  "other": null
}
```

### API Versioning
- URI-based versioning: `/api/v1/`
- See [ADR-0005](/project-context/adr/ADR-0005-versioning-strategy.md)

---

## Project Structure

```
flowingly-technical-test/
├── src/
│   ├── Api/                  # REST API endpoints, DI, middleware
│   ├── Application/          # CQRS handlers, validators
│   │   ├── Commands/         # Command handlers (write operations)
│   │   ├── Queries/          # Query handlers (read operations)
│   │   ├── Handlers/         # Business logic execution
│   │   └── Validators/       # FluentValidation rules
│   ├── Domain/               # Core domain logic (parsers, calculators)
│   │   ├── Parsers/          # Tag/XML/number/date/time parsers
│   │   ├── Validators/       # Tag integrity validation
│   │   ├── Calculators/      # GST tax calculation
│   │   ├── ValueObjects/     # Domain value types
│   │   └── Interfaces/       # Ports (abstractions)
│   └── Infrastructure/       # Adapters (EF Core, repositories)
│       ├── Persistence/      # EF Core DbContext, configurations
│       ├── Repositories/     # Data access implementations
│       ├── Logging/          # Serilog setup
│       └── Configuration/    # App settings
├── client/                   # React + Vite frontend
│   ├── src/
│   │   ├── components/       # UI components
│   │   ├── api/              # API client/fetch wrappers
│   │   └── types/            # TypeScript interfaces
│   └── tests/                # Playwright E2E tests
├── contracts/                # Shared request/response DTOs
├── tests/                    # Backend test projects
├── scripts/                  # Development automation scripts
└── project-context/          # Specifications, ADRs, planning docs
    ├── adr/                  # Architectural Decision Records
    ├── specifications/       # PRD, technical specs
    ├── planning/             # Delivery plan, task decomposition
    └── implementation/       # Progress tracking, task files
```

---

## Tech Stack

### Backend
- **.NET 8** (Minimal API)
- **xUnit** (Testing framework)
- **FluentAssertions** (Test assertions)
- **FluentValidation** (Request validation)
- **Serilog** (Structured logging with correlation IDs)
- **Entity Framework Core** (ORM)
- **SQLite** (Local development database)
- **PostgreSQL** (Production database on Render)
- **Swashbuckle** (Swagger/OpenAPI documentation)

### Frontend
- **React 18**
- **Vite** (Build tool)
- **TypeScript** (Type safety)
- **Playwright** (E2E testing)

### Deployment
- **Platform**: Render (free tier)
- **Database**: Render PostgreSQL (free tier)
- **CI/CD**: GitHub Actions with Render integration

---

## Key Features & Design Decisions

### Tax Calculation (ADR-0009)
- Computes NZ GST from tax-inclusive totals
- Uses Banker's Rounding (`MidpointRounding.ToEven`)
- Formula: `totalExcludingGst = total / 1.15`, `gst = total - totalExcludingGst`

### Validation Rules (ADR-0008)
- `<total>` (tax-inclusive) is **required** for expenses; reject if missing
- `<cost_centre>` is **optional**; default to `UNKNOWN` if absent
- Any unclosed tag → **reject with 400 error**
- Stack-based tag validation ensures integrity

### Content Routing (ADR-0003)
- Strategy Pattern: Route content to ExpenseProcessor or OtherProcessor
- Expense: Parse tags, extract XML, compute GST, return structured data
- Other: Store unprocessed text for future processors (e.g., reservations)

### Response Contract (ADR-0007)
- XOR design: `expense` XOR `other` (never both)
- Always includes `correlationId` for traceability
- Expense response includes GST breakdown

---

## Progress Tracking

This project uses a **50-task decomposition system** with 4 milestone gates:

- **M0: Minimal Scaffold** (10 tasks, 4 hours) — ✅ Complete
- **M1: Core Parsing & Validation** (20 tasks, 1 day) — 🔄 In Progress
- **M2: API Contracts** (10 tasks, 4 hours) — 📋 Planned
- **M3: UI & E2E Tests** (10 tasks, 4 hours) — 📋 Planned

**Track Progress**:
- Dashboard: `project-context/implementation/PROGRESS.md`
- Detailed logs: `project-context/build-logs/BUILDLOG.md`
- Task files: `project-context/implementation/tasks/`

**Update Progress**:
```bash
# Start a task
./scripts/update-progress.sh task_XXX in_progress

# Complete a task
./scripts/update-progress.sh task_XXX completed

# Complete with test counts
./scripts/update-progress.sh task_014 completed unit 7
```

---

## MCP Servers & Enhanced Development Tools

This project uses **Model Context Protocol (MCP) servers** for enhanced development capabilities. See `project-context/learnings/serena-mcp-guide.md` for comprehensive guides.

### Serena MCP (Semantic Code Analysis)
✅ Configured and indexed for C# codebase

**Use for**:
- Exploring code structure (classes, methods, interfaces)
- Finding all references to a symbol
- Refactoring (renaming, changing signatures)
- Symbol-based code editing

**Example**:
```bash
Get symbols overview for src/Domain/Services/TaxCalculator.cs
Find symbol "TaxCalculator/CalculateFromInclusive" with include_body=true
Replace symbol body "ExpenseProcessor/ProcessAsync" with [new implementation]
```

### Context7 MCP (Library Documentation)
Fetch up-to-date documentation for .NET, React, and other libraries beyond Claude's knowledge cutoff.

### Playwright MCP (Browser Automation)
Write and debug E2E tests with browser automation (critical for M3 milestone).

### IDE MCP (VS Code Integration)
Get compiler diagnostics and execute code in development environment.

---

## Documentation

### Key Documents
- **Specifications**: `project-context/specifications/prd-technical_spec.md` (v0.3)
- **ADRs**: `project-context/adr/` (10 architectural decisions)
- **Build Log**: `project-context/build-logs/BUILDLOG.md`
- **Task System**: `project-context/implementation/tasks/`
- **MCP Guides**: `project-context/learnings/serena-mcp-guide.md`

### Git Conventions
Use **conventional commits** syntax:
```bash
git commit -m "feat(api): add expense parsing endpoint"
git commit -m "test(unit): add tax calculator tests"
git commit -m "docs(readme): update quick start guide"
```

Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `perf`, `style`

---

## Contributing

### Workflow
1. Check task dependencies: `project-context/implementation/tasks/tasks.json`
2. Create task file if missing: Follow `TASK_CREATION_GUIDE.md`
3. Mark in-progress: `./scripts/update-progress.sh task_XXX in_progress`
4. Implement following TDD cycles (for M1 tasks)
5. Run tests and verify coverage
6. Mark completed: `./scripts/update-progress.sh task_XXX completed [test_type] [count]`
7. Commit with conventional commits
8. Update BUILDLOG.md with changes and rationale

### Code Principles
- Follow SOLID, DRY, KISS, YAGNI
- Simple, maintainable, extensible over complex
- Verify against specification before implementing
- Ensure type safety: align database schema, application types, and TypeScript interfaces

---

## License

This is a technical test project for Flowingly. See test brief in `project-context/requirements-and-analysis/` for context.

---

## Support & Questions

- **Specifications**: See `project-context/specifications/prd-technical_spec.md`
- **Architecture Decisions**: See `project-context/adr/`
- **Implementation Status**: See `project-context/implementation/PROGRESS.md`
- **Build History**: See `project-context/build-logs/BUILDLOG.md`

**Happy Coding!** 🚀

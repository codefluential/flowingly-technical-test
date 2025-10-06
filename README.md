# Flowingly Parsing Service

A modular text-ingestion and parsing service that extracts structured data from free-form text containing inline tags and XML islands, starting with Expense Claims processing.

**Current Status**: ✅ M3 Complete (51/51 tasks, 100%) — Production Ready & Submission Complete

**Latest**: All 3 sample emails passing • 150 tests (116 unit + 13 contract + 21 E2E) • Accessible UI • Ready for deployment

## Quick Start

### Prerequisites

**Required Software**:
- **[.NET 8 SDK](https://dotnet.microsoft.com/download)** - Cross-platform runtime (.NET CLI)
- **[Node.js 18+](https://nodejs.org/)** - JavaScript runtime (includes npm)
- **[Git](https://git-scm.com/downloads)** - Version control

**Platform Notes**:
- ✅ **Windows**: Installers automatically configure PATH. No manual setup needed.
- ✅ **macOS**: Installers automatically configure PATH. No manual setup needed.
- ⚠️ **Linux**: If `dotnet` or `npm` commands not found after install, see [Manual PATH Configuration](#manual-path-configuration) (Appendix A).

### 1. Clone Repository

```bash
git clone <repository-url>
cd flowingly-technical-test
```

> **Works on**: Windows (PowerShell/cmd/Git Bash), Linux, macOS

---

### 2. Run Backend API

**Terminal 1 - Start Backend**:
```bash
dotnet run --project src/Api
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Verify**:
- Open http://localhost:5000/swagger in browser
- You should see Swagger UI with the `/api/v1/parse` endpoint

> **Works on**: Windows, Linux, macOS (no PATH config needed if installers used)

---

### 3. Run Frontend

**Terminal 2 - Start Frontend**:
```bash
cd client
npm install
npm run dev
```

**Expected Output**:
```
  VITE v7.1.9  ready in 245 ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
```

**Verify**:
- Open http://localhost:5173 in browser
- You should see the parsing service UI with text input

> **Works on**: Windows, Linux, macOS

### 4. Test the Application

**Option 1 - Swagger UI (Recommended)**:
1. Open http://localhost:5000/swagger
2. Click POST `/api/v1/parse` → "Try it out"
3. Paste sample expense text:
   ```json
   {
     "text": "Hi Yvaine, Please create an expense claim. <vendor>Mojo Coffee</vendor> <total>120.50</total> <cost_centre>DEV</cost_centre>",
     "taxRate": 0.15
   }
   ```
4. Click "Execute" → Verify parsed expense with GST calculation

**Option 2 - Frontend UI**:
1. Open http://localhost:5173
2. Paste sample expense text in textarea
3. Click "Parse" → See parsed expense data
4. Verify correlation ID in response

**Sample Test Inputs** (from test brief):
```
Sample 1 (XML Island):
Hi Yvaine, Please create an expense claim for the below. Relevant details are:
<expense><cost_centre>DEV002</cost_centre><total>1024.01</total><payment_method>personal card</payment_method></expense>

Sample 2 (Inline Tags):
Hi Yvaine, Please create an expense claim for the below. Relevant details are:
<vendor>Mojo Coffee</vendor> <total>120.50</total> <payment_method>personal card</payment_method>

Sample 3 (Error - Missing Total):
Hi Yvaine, Please create an expense claim for the below:
<vendor>Starbucks</vendor> <payment_method>personal card</payment_method>
→ Returns 400 Bad Request with MISSING_TOTAL error code
```

✅ **Setup complete in under 5 minutes!**

---

## What This Service Does

**Core Objective**: Accept raw text (e.g., email bodies), validate tag integrity, extract expense data, compute NZ GST tax breakdowns (from inclusive totals), and return normalized JSON.

**Current Implementation** (✅ M3 Complete - Production Ready):
- ✅ Tag validation (stack-based, rejects unclosed tags)
- ✅ XML island extraction (secure parsing, DTD/XXE disabled)
- ✅ Expense data extraction with GST calculation
- ✅ Number/date/time normalization
- ✅ Content routing (Expense vs Other/Unprocessed)
- ✅ RESTful API with Swagger documentation
- ✅ Error handling with structured responses
- ✅ Enhanced React UI with error display and accessibility features
- ✅ TypeScript types for type-safe API integration
- ✅ 150 tests passing (116 unit + 13 contract + 21 E2E)
- ✅ All 3 sample emails from test brief working
- ✅ Production deployment ready (Render configuration)

---

## Test Brief Compliance ✅

**Status**: ✅ MEETS ALL MINIMUM REQUIREMENTS

This implementation satisfies all requirements from the Full Stack Engineer Test (Sen) V2 test brief:

### ✅ Core Functionality
- Parse expense emails and extract structured data
- Handle inline tags (`<vendor>Test</vendor> <total>100</total>`)
- Handle XML islands (`<expense><total>100</total></expense>`)
- Calculate NZ GST from tax-inclusive totals (15% rate)
- Validate required fields (`<total>` is required)
- Handle missing/invalid data gracefully with error codes

### ✅ Sample Emails (All 3 Passing)
| Sample | Input | Expected Output | Status |
|--------|-------|-----------------|--------|
| **1** | XML Island with `<expense>` tag | Parse DEV002, $1024.01 total, $133.57 GST | ✅ Passing |
| **2** | Inline tags for vendor/total | Parse Mojo Coffee, $120.50 total, $15.72 GST | ✅ Passing |
| **3** | Missing `<total>` tag | Return 400 with MISSING_TOTAL error | ✅ Passing |

### ✅ Technical Requirements
- ✅ .NET 8 RESTful API with JSON request/response
- ✅ React frontend with form input and result display
- ✅ Swagger/OpenAPI documentation at `/swagger`
- ✅ Error handling with structured responses
- ✅ Validation logic (tag integrity, required fields)
- ✅ GST calculation with Banker's Rounding
- ✅ Well-structured, maintainable code (Clean Architecture)
- ✅ Comprehensive test coverage (129 tests, 287% of target)
- ✅ Clear documentation (README, ADRs, Swagger)

**Verification**: See `TEST-BRIEF-COMPLIANCE.md` for detailed compliance report and demo instructions.

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

**All commands work on Windows, Linux, and macOS**:

```bash
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

> **Note**: If `dotnet` command not found, see [Manual PATH Configuration](#manual-path-configuration) (Appendix A).

### Frontend Commands

**All commands work on Windows, Linux, and macOS**:

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

# Run E2E tests (M3 milestone - not yet implemented)
npm run test:e2e

# Run specific E2E test (M3 milestone)
npx playwright test path/to/test.spec.ts
```

> **Note**: Commands work in PowerShell, cmd.exe, bash, zsh. For Git Bash on Windows, see [Appendix B](#appendix-b-git-bash-on-windows).

---

## Testing

**Current Status**: ✅ 150/45 tests passing (333% of target)

**Coverage Achieved** ([ADR-0010](/project-context/adr/ADR-0010-test-strategy-coverage.md)):
- ✅ 116/30 unit tests (387% of target) - Domain/Application logic
- ✅ 13/10 contract tests (130% of target) - API integration
- ✅ 21/5 E2E tests (420% of target) - Playwright browser tests
- **Total: 150 tests passing** (exceeds all targets across all milestones)

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
npm run test:e2e  # 21 E2E tests with Playwright
```

**All Tests**:
```bash
dotnet test  # Backend (129 tests: 116 unit + 13 contract)
cd client && npm run test:e2e  # Frontend E2E (21 tests)
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
  "text": "Hi Yvaine, Please create an expense claim for the below. Relevant details are: <expense><cost_centre>DEV002</cost_centre><total>1024.01</total><payment_method>personal card</payment_method></expense>",
  "taxRate": 0.15
}
```

**Success Response (Expense)**:
```json
{
  "classification": "expense",
  "expense": {
    "vendor": null,
    "description": null,
    "total": 1024.01,
    "totalExclTax": 890.44,
    "salesTax": 133.57,
    "costCentre": "DEV002",
    "date": null,
    "time": null,
    "paymentMethod": "personal card"
  },
  "meta": {
    "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "warnings": [],
    "tagsFound": ["cost_centre", "total", "payment_method"]
  }
}
```

**Success Response (Other/Unprocessed)**:
```json
{
  "classification": "other",
  "other": {
    "rawTags": {},
    "note": "Content stored for future processing"
  },
  "meta": {
    "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "warnings": [],
    "tagsFound": []
  }
}
```

**Error Response (Validation Failure)**:
```json
{
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "errorCode": "MISSING_TOTAL",
  "message": "<total> tag is required for expense processing",
  "details": null
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
- **React 19**
- **Vite** (Build tool)
- **TypeScript** (Type safety)
- **Playwright** (E2E testing - M3 milestone)

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

This project uses a **51-task decomposition system** with 4 milestone gates:

- **M0: Minimal Scaffold** (10 tasks, 4 hours) — ✅ Complete
- **M1: Core Parsing & Validation** (20 tasks, 1 day) — ✅ Complete (116 unit tests)
- **M2: API Contracts** (10 tasks, 4 hours) — ✅ Complete (13 contract tests)
- **M3: UI & E2E Tests** (11 tasks, 4 hours) — ✅ Complete (21 E2E tests + accessible UI)

**Current Progress**: 51/51 tasks (100%) • **Ready for submission**

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
3. Mark in-progress: `./scripts/update-progress.sh task_XXX in_progress` (Unix/Git Bash/WSL)
4. Implement following TDD cycles (for M1 tasks)
5. Run tests and verify coverage
6. Mark completed: `./scripts/update-progress.sh task_XXX completed [test_type] [count]` (Unix/Git Bash/WSL)
7. Commit with conventional commits
8. Update BUILDLOG.md with changes and rationale

> **Windows Users**: See [Appendix D](#appendix-d-development-scripts-unixlinuxmacos) for script compatibility options.

### Code Principles
- Follow SOLID, DRY, KISS, YAGNI
- Simple, maintainable, extensible over complex
- Verify against specification before implementing
- Ensure type safety: align database schema, application types, and TypeScript interfaces

---

## Appendix: Platform-Specific Notes

### Appendix A: Manual PATH Configuration

**Only needed if** you see `dotnet: command not found` or `npm: command not found` errors after installation.

<details>
<summary><b>Windows (PowerShell)</b> - Rare, installers handle this automatically</summary>

If using a portable .NET installation (not recommended):

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet"
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
```

To persist across sessions, add to PowerShell profile:
```powershell
notepad $PROFILE
# Add the two lines above, save, restart PowerShell
```

**Recommended**: Use the official installer from https://dotnet.microsoft.com/download which configures PATH automatically.

</details>

<details>
<summary><b>Linux (Bash)</b></summary>

Add to `~/.bashrc`:

```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH="$DOTNET_ROOT:$PATH"
```

Apply changes:
```bash
source ~/.bashrc
```

Verify:
```bash
dotnet --version
# Should output: 8.0.xxx
```

</details>

<details>
<summary><b>macOS (Zsh)</b> - Default shell on macOS 10.15+</summary>

Add to `~/.zshrc`:

```zsh
export DOTNET_ROOT=$HOME/.dotnet
export PATH="$DOTNET_ROOT:$PATH"
```

Apply changes:
```zsh
source ~/.zshrc
```

**Note**: macOS installers typically configure PATH automatically. Only use this if `dotnet` command not found.

</details>

---

### Appendix B: Git Bash on Windows

**What is Git Bash?**
- Unix-like bash shell included with Git for Windows
- Provides Linux/macOS command compatibility on Windows
- Recommended for Windows users familiar with bash

**Advantages**:
- ✅ All Linux/macOS commands work identically
- ✅ Supports shell scripts (`./scripts/*.sh`)
- ✅ Unix path separators work (`/home/user/`)
- ✅ Bash features like `&&`, `||`, pipes

**Installation**:
1. Download Git for Windows: https://git-scm.com/download/win
2. During install, select "Git Bash Here" context menu option
3. Launch via Start Menu → "Git Bash"

**Usage**:
```bash
# In Git Bash, all README commands work as-is
git clone <url>
cd flowingly-technical-test
dotnet run --project src/Api
```

---

### Appendix C: Shell-Specific Command Syntax

<details>
<summary><b>Windows cmd.exe Users</b></summary>

**Chained Commands**:

Most README examples use `&&` (bash/PowerShell):
```bash
cd client && npm run dev
```

In Windows cmd.exe, use single `&`:
```cmd
cd client & npm run dev
```

**Path Separators**:

Both forward slashes and backslashes work in modern .NET:
```cmd
dotnet run --project src/Api
dotnet run --project src\Api
```
Both commands are equivalent on Windows.

</details>

<details>
<summary><b>PowerShell Users</b></summary>

**PowerShell 7+** supports most bash syntax:
- ✅ `&&` for chained commands
- ✅ Forward slashes in paths
- ✅ Most Unix-style commands

**PowerShell 5.1** (Windows default):
- Use `;` instead of `&&` for chained commands:
  ```powershell
  cd client; npm run dev
  ```

**Check PowerShell version**:
```powershell
$PSVersionTable.PSVersion
```

</details>

---

### Appendix D: Development Scripts (Unix/Linux/macOS)

**Shell Scripts Location**: `scripts/`

These bash scripts are for development workflow automation:

```bash
./scripts/update-progress.sh task_001 completed
./scripts/reindex-serena.sh
./scripts/verify-dod.sh M0
```

**Windows Users - Options**:

1. **Use Git Bash** (Recommended):
   ```bash
   # Scripts work identically in Git Bash
   ./scripts/update-progress.sh task_001 completed
   ```

2. **Use WSL (Windows Subsystem for Linux)**:
   ```bash
   # Run in WSL terminal
   ./scripts/update-progress.sh task_001 completed
   ```

3. **Manual Execution**:
   - View script contents to see underlying commands
   - Execute commands manually in PowerShell/cmd.exe

> **Note**: These scripts are for development workflow automation, **not required** to build and run the application. Core commands (`dotnet run`, `npm run dev`) work natively on Windows.

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

# Context Reset Prompt — Resume Task Execution

**Use this prompt after `/clear` to resume work on the next task(s).**

---

## Quick Start Command

```
Check progress status and start the next pending task(s) according to tasks.json dependencies and parallel execution groups.
```

---

## Project Context (Essential)

**Flowingly Parsing Service** — Text-ingestion service extracting structured expense data from free-form text with inline tags/XML islands. Computes NZ GST tax breakdowns (from inclusive totals), returns normalized JSON.

### Current State
- **Phase**: Phase 1 (M0→M3) — Core Submission
- **Objective**: Meet test brief requirements in 2.5 days
- **Status**: Check `project-context/implementation/PROGRESS.md` for current task
- **Test Brief**: `project-context/requirements-and-analysis/Full Stack Engineer Test (Sen) V2.pdf`

### Key Technical Requirements
- **Stack**: .NET 8 Web API (Clean/Hexagonal Architecture) + React+Vite+TypeScript
- **Parsing Rules**: Stack-based tag validation, Banker's Rounding (ToEven), GST calculation
- **Testing**: 45+ tests (30 unit, 10 contract, 5 E2E) — TDD approach for M1
- **Submission**: After task_050 (M3 DoD gate)

---

## Task Execution Workflow

### 1. Check Current Status
```bash
# View progress dashboard
cat project-context/implementation/PROGRESS.md

# Get current task from tracking
jq '.progress_tracking.current_task' project-context/implementation/tasks/tasks.json

# Check which tasks are ready (dependencies met)
jq '.tasks[] | select(.status == null or .status == "pending") | select(.dependencies == [] or .dependencies | length == 0) | {id, name, agent, milestone}' project-context/implementation/tasks/tasks.json
```

### 2. Load Task Context
```bash
# If task file exists, read it for full context
cat project-context/implementation/tasks/task_XXX.json

# If task file missing, check metadata from tasks.json
jq '.tasks[] | select(.id == "task_XXX")' project-context/implementation/tasks/tasks.json
```

**Task File Creation** (if missing):
- Extract metadata from `tasks.json`
- Use `project-context/implementation/tasks/TASK_CREATION_GUIDE.md` as template
- Reference PRD sections, ADRs, delivery plan as specified in guide
- Validate against quality checklist
- Commit task file before starting implementation

### 3. Execute Task
```bash
# Mark in-progress
./scripts/update-progress.sh task_XXX in_progress

# Execute following task's acceptance criteria
# Use specialized agent if assigned in tasks.json

# Mark completed with test counts (if applicable)
./scripts/update-progress.sh task_XXX completed [unit|contract|e2e] [count]
```

### 4. Commit Work
```bash
# Commit implementation separately from progress tracking
git add [implementation files]
git commit -m "feat(scope): task_XXX description"

# Commit progress files separately
git add project-context/implementation/
git commit -m "chore(progress): complete task_XXX"
```

---

## Key Architecture Reminders

### Clean/Hexagonal Layers
- **Api/**: Endpoints, DI, Swagger, Middleware
- **Application/**: Commands/Queries, Handlers (CQRS-lite)
- **Domain/**: Parsers, Calculators, Interfaces (Ports) — Pure functions, no dependencies
- **Infrastructure/**: EF Core, Repositories (Adapters)

### Critical Parsing Components
- `ITagValidator`: Stack-based tag integrity (reject overlapping/unclosed tags)
- `ITaxCalculator`: GST computation with Banker's Rounding (`MidpointRounding.ToEven`)
- `IXmlIslandExtractor`: Secure XML (DTD/XXE disabled)
- `INumberNormalizer`: Currency/comma stripping, decimal precision

### Validation Rules
- `<total>` required for expenses → `MISSING_TOTAL` error if absent
- `<cost_centre>` optional → default `"UNKNOWN"` if absent
- Unclosed/overlapping tags → `UNCLOSED_TAGS` error (400)

---

## TDD Workflow (M1 Tasks Only)

**RED → GREEN → REFACTOR cycles**

Example sequence:
- task_014 (RED): Write tag validation tests (failing)
- task_015 (GREEN): Implement `ITagValidator` (tests pass)
- task_016 (REFACTOR): Code quality review

**Test Pattern**:
1. `tdd-london-swarm` agent writes failing tests (xUnit + FluentAssertions)
2. `coder` agent implements to pass tests
3. `code-analyzer` agent reviews quality (optional refactor step)

---

## Parallel Execution Groups

Check `tasks.json` for parallel-safe tasks:
- **M0_parallel_1**: task_003 (API) + task_004 (React) — after task_002
- **M1_parallel_1**: task_014, 017, 019 — after task_013
- **M1_parallel_2**: task_021, 023, 025 — after task_020
- **M2_parallel_1**: task_037 (tests) + task_038 (Swagger) — after task_036
- **M3_parallel_1**: task_045, 046, 047 (all E2E tests) — after task_044

**Rule**: Only execute parallel tasks if dependencies are met AND tasks are independent.

---

## Milestone Gates (DoD Tasks)

**Critical checkpoints** before proceeding to next milestone:
- **task_010**: M0 DoD → Verify echo flow works, README ready
- **task_030**: M1 DoD → 30+ unit tests green, all parsers implemented
- **task_040**: M2 DoD → API contracts working, 10+ contract tests green
- **task_050**: M3 DoD → **SUBMITTABLE** (45+ tests, UI functional, E2E tests green)

**DoD tasks auto-update** `BUILDLOG.md` when completed.

---

## Essential Documents Reference

**Specification & Requirements**:
- PRD: `project-context/specifications/prd-technical_spec.md` (v0.3)
- Test Brief: `project-context/requirements-and-analysis/Full Stack Engineer Test (Sen) V2.pdf`
- Delivery Plan: `project-context/planning/delivery-plan-optimized.md`

**Architecture Decisions**:
- ADR Directory: `project-context/adr/`
- Key ADRs: 0002 (architecture), 0007 (response contract), 0008 (parsing rules), 0009 (Banker's Rounding), 0010 (test strategy)

**Progress Tracking**:
- Master orchestration: `project-context/implementation/tasks/tasks.json`
- Dashboard: `project-context/implementation/PROGRESS.md`
- Build history: `project-context/build-logs/BUILDLOG.md`

**Task Creation**:
- Guide: `project-context/implementation/tasks/TASK_CREATION_GUIDE.md`

---

## MCP Tools Available

**Serena MCP** (Semantic C# analysis):
- `get_symbols_overview` — Explore file structure
- `find_symbol` — Find classes/methods by name
- `find_referencing_symbols` — Find all references
- `replace_symbol_body` — Edit by symbol (not line numbers)

**Context7 MCP** (Library docs):
- Fetch up-to-date .NET/React documentation

**Playwright MCP** (E2E testing, M3):
- Browser automation for E2E test verification

---

## Common Commands

**Backend**:
```bash
dotnet build
dotnet test
dotnet run --project src/Api  # API on :5001
```

**Frontend**:
```bash
npm install
npm run dev                    # UI on :5173
npm run test:e2e              # Playwright E2E tests
```

**Progress Tracking**:
```bash
./scripts/update-progress.sh task_XXX in_progress
./scripts/update-progress.sh task_XXX completed [test_type] [count]
```

---

## What To Do Next

1. **Check progress**: `cat project-context/implementation/PROGRESS.md`
2. **Identify next task(s)**: Review dependencies in `tasks.json`
3. **Load task context**: Read task file if exists, or create using TASK_CREATION_GUIDE.md
4. **Execute**: Mark in-progress → implement → test → mark completed
5. **Commit**: Implementation separately, then progress files
6. **Repeat**: Move to next task in sequence

---

## Success Criteria (Phase 1 Submission)

**Ready to submit after task_050 when**:
✅ All 50 Phase 1 tasks completed
✅ 45+ tests green (30 unit + 10 contract + 5 E2E)
✅ Sample emails from test brief parse correctly
✅ Clone → `dotnet run` + `npm run dev` → verify in <5 min
✅ README has quick-start guide

---

**Last Updated**: 2025-10-06
**Current Milestone**: M0 (50% complete — 5/10 tasks done)
**Next Task**: task_005 (Create API Client)

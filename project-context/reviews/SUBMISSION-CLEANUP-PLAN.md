# Submission Cleanup Plan

**Purpose**: Prepare clean submission branches for recruiter while preserving full development history locally.

**Strategy**: Create a "submission-ready" branch from dev with AI/process artifacts removed, keep full history in local-only "archive" branch.

---

## 📋 Current State Analysis

### Repository Structure
```
flowingly-technical-test/
├── .claude/                    ← AI config (19 agents, settings)
├── .serena/                    ← MCP server cache (gitignored but present)
├── .playwright-mcp/            ← Playwright MCP state
├── project-context/            ← MASSIVE: planning, learnings, reviews, implementation tracking
├── src/                        ← Core .NET code ✅ KEEP
├── client/                     ← React frontend ✅ KEEP
├── tests/                      ← xUnit + E2E tests ✅ KEEP
├── contracts/                  ← DTOs ✅ KEEP
├── fixtures/                   ← Test data ✅ KEEP
├── scripts/                    ← Dev automation (update-progress.sh, etc.)
├── CLAUDE.md                   ← AI instructions (22KB)
├── README.md                   ← Main docs ✅ KEEP (with edits)
├── TEST-BRIEF-COMPLIANCE.md    ← Requirements mapping ✅ KEEP
└── .github/workflows/          ← CI/CD (if exists)
```

### What Recruiters Need to See
✅ **Working application** (src/, client/, tests/)
✅ **Professional documentation** (README, ADRs, architecture)
✅ **Test coverage** (unit, integration, E2E tests)
✅ **Requirements traceability** (TEST-BRIEF-COMPLIANCE.md)
✅ **Build/run instructions** (simplified README)

### What Recruiters DON'T Need
❌ **AI orchestration artifacts** (.claude/, CLAUDE.md, agent files)
❌ **Process tracking** (tasks.json, PROGRESS.md, update-progress.sh)
❌ **Internal learnings** (implementation insights, duration analysis)
❌ **Planning documents** (delivery plans, agent resourcing)
❌ **Code review notes** (M3_CODE_REVIEW_FINDINGS.md)
❌ **MCP server state** (.serena/, .playwright-mcp/)
❌ **Verbose build logs** (BUILDLOG.md - keep condensed version)

---

## 🎯 Three-Branch Strategy

### Branch 1: `archive/full-development-history` (Local Only)
**Purpose**: Preserve EVERYTHING for post-interview reference
**Location**: Local only (never push to GitHub)
**Contents**: Current dev branch as-is (100% fidelity)

```bash
# Create from current dev
git checkout dev
git checkout -b archive/full-development-history
git push --set-upstream origin archive/full-development-history  # Optional: backup to private remote later
```

### Branch 2: `submission` (Clean for Recruiter)
**Purpose**: Production-ready submission with minimal docs
**Location**: Push to GitHub (public or private)
**Contents**: Code + essential docs only

### Branch 3: `main` (Update from submission)
**Purpose**: Default branch for GitHub visitors
**Location**: Push to GitHub
**Contents**: Same as submission branch

---

## 📂 Detailed Cleanup Plan

### KEEP (Essential for Submission)

#### 1. Application Code ✅
```
src/                           # .NET backend (all layers)
client/src/                    # React frontend
tests/                         # All tests (unit, integration, E2E)
contracts/                     # Shared DTOs
fixtures/                      # Test data (sample emails)
```

#### 2. Core Documentation ✅
```
README.md                      # Quick start + architecture (EDIT: remove AI references)
TEST-BRIEF-COMPLIANCE.md       # Requirements mapping
project-context/
  ├── adr/                     # All 10 ADRs ✅
  ├── specifications/
  │   └── prd-technical_spec.md  # PRD v0.3 ✅
  ├── requirements-and-analysis/
  │   └── Full Stack Engineer Test (Sen) V2.pdf  # Original brief ✅
  └── build-logs/
      └── BUILDLOG.md          # CONDENSED VERSION (remove verbose task tracking)
```

#### 3. Build System ✅
```
Flowingly.ParsingService.sln   # .NET solution
src/Api/appsettings.json       # Config (keep Development version too)
client/package.json            # Frontend deps
client/tsconfig*.json          # TypeScript config
.gitignore                     # Standard ignores
```

#### 4. Minimal Scripts ✅
```
scripts/
  ├── build.sh                 # Build script
  ├── test.sh                  # Test runner
  ├── dev.sh                   # Dev server
  └── dev.ps1                  # Windows dev script
```

---

### REMOVE (AI/Process Artifacts)

#### 1. AI Configuration ❌
```
.claude/                       # All Claude Code config + 19 agents
CLAUDE.md                      # 22KB AI instructions
.serena/                       # MCP server cache
.playwright-mcp/               # Playwright MCP state
```

#### 2. Process Tracking ❌
```
project-context/
  ├── implementation/          # All task tracking (tasks.json, PROGRESS.md, etc.)
  ├── planning/                # Delivery plans, agent resourcing
  ├── learnings/               # Implementation insights
  ├── reviews/                 # Code review notes, Windows testing
  ├── codex/                   # Codex reviews
  ├── agents/                  # Agent definitions
  └── archive/                 # General notes
```

#### 3. Automation Scripts ❌
```
scripts/
  ├── update-progress.sh       # Task tracking automation
  ├── backfill-task-durations.sh
  ├── reindex-serena.sh        # MCP indexing
  ├── FIXES-SUMMARY.md
  ├── TEST-SIGNAL-TRAP.md
  └── VALIDATION.md
```

#### 4. Miscellaneous ❌
```
package.json                   # Root-level (only used for scripts/run-script.js)
scripts/run-script.js
client/project-context/        # If symlink exists
api/                           # If exists (seems unused)
```

---

## 🔧 Step-by-Step Execution Plan

### Phase 1: Create Archive Branch (5 min)

```bash
# Ensure all work committed
git status  # Should be clean

# Create archive from dev
git checkout dev
git checkout -b archive/full-development-history

# Optional: Tag for reference
git tag -a v1.0-full-dev-archive -m "Complete development history with AI artifacts"

# Stay local (do NOT push to public GitHub)
# Later, after interview, push to private repo for backup
```

---

### Phase 2: Create Submission Branch (15 min)

```bash
# Create submission branch from dev
git checkout dev
git checkout -b submission

# Remove AI artifacts
rm -rf .claude/
rm -f CLAUDE.md
rm -rf .serena/
rm -rf .playwright-mcp/

# Remove process tracking
rm -rf project-context/implementation/
rm -rf project-context/planning/
rm -rf project-context/learnings/
rm -rf project-context/reviews/
rm -rf project-context/codex/
rm -rf project-context/agents/
rm -rf project-context/archive/

# Remove automation scripts (keep essential build/test scripts)
rm -f scripts/update-progress.sh
rm -f scripts/backfill-task-durations.sh
rm -f scripts/reindex-serena.sh
rm -f scripts/FIXES-SUMMARY.md
rm -f scripts/TEST-SIGNAL-TRAP.md
rm -f scripts/VALIDATION.md
rm -f scripts/run-script.js

# Remove root package.json (only used for task automation)
rm -f package.json

# Remove unused directories
rm -rf api/  # If exists
rm -rf client/project-context/  # If symlink

# Condense BUILDLOG.md (keep high-level milestones only)
# Manual edit required - see below

# Clean README.md (remove AI references)
# Manual edit required - see below

# Stage all deletions
git add -A

# Commit cleanup
git commit -m "chore: prepare submission branch - remove AI/process artifacts

- Remove .claude/, CLAUDE.md (AI orchestration)
- Remove project-context/implementation, planning, learnings (process tracking)
- Remove task automation scripts (update-progress.sh, etc.)
- Remove MCP server state (.serena/, .playwright-mcp/)
- Condense BUILDLOG.md to high-level milestones
- Clean README.md (remove AI workflow references)

Retained:
- All application code (src/, client/, tests/)
- Core documentation (ADRs, PRD, test brief)
- Essential scripts (build.sh, test.sh, dev.sh)
- Test fixtures and contracts

Full development history preserved in archive/full-development-history branch (local)"
```

---

### Phase 3: Manual Edits

#### A. Condense BUILDLOG.md

**Current**: 356 lines with verbose task tracking, duration metrics, test counts

**Target**: ~100 lines with high-level milestone summaries

**Edit `project-context/build-logs/BUILDLOG.md`**:

```markdown
# Build Log — Flowingly Parsing Service

**Purpose**: High-level development milestones and key decisions.

---

## M0: Minimal Scaffold — 2025-10-06

**Milestone**: End-to-end application scaffold
**Duration**: ~4 hours

### Key Achievements
- .NET 8 solution with Clean Architecture (Api, Application, Domain, Infrastructure)
- React 19 + Vite + TypeScript frontend with basic parsing UI
- Swagger documentation at `/swagger`
- CORS configuration for local development
- Structured logging with correlation IDs

### Technical Decisions
- Clean Architecture for testability (ADR-0002)
- Swagger for self-documenting API (ADR-0004)

---

## M1: Core Parsing & Validation — 2025-10-06

**Milestone**: TDD implementation of parsing pipeline
**Duration**: ~6 hours

### Key Achievements
- Stack-based tag validation (rejects unclosed/overlapping tags)
- Banker's Rounding for GST calculations (ADR-0009)
- Secure XML parsing (DTD/XXE disabled)
- Tax calculator with rate precedence (request > config > fallback)
- Time parser with whitelist validation (rejects ambiguous formats)
- Content router with Strategy pattern
- Complete expense processor pipeline

### Test Coverage
- 116 unit tests covering all domain logic
- TDD approach: RED → GREEN → REFACTOR cycles
- All 3 sample emails from test brief verified

### Technical Decisions
- Banker's Rounding for financial accuracy (ADR-0009)
- XML security hardening against XXE attacks (ADR-0008)
- Stack-based validation over regex for nested tags (ADR-0008)

---

## M2: API Contracts — 2025-10-06

**Milestone**: RESTful API with contract tests
**Duration**: ~4 hours

### Key Achievements
- POST `/api/v1/parse` endpoint with FluentValidation
- XOR response contract (expense OR other, not both) per ADR-0007
- Exception mapping middleware (400 for validation, 500 for internal errors)
- Correlation IDs in all responses
- Processing time tracking (Stopwatch)
- 13 contract tests covering happy paths and error scenarios

### Technical Decisions
- URI versioning (`/api/v1/`) for simplicity (ADR-0005)
- XOR response structure for type safety (ADR-0007)
- API key authentication for production (ADR-0006)

---

## M3: UI & E2E Tests — 2025-10-07

**Milestone**: Enhanced UI and browser automation tests
**Duration**: ~4 hours

### Key Achievements
- Enhanced UI with all expense fields (payment method, date, time, currency, source)
- Error display component with structured error codes
- TypeScript types aligned with backend contracts
- Playwright E2E tests (9 tests covering happy paths, error scenarios, GST verification)
- 138 total tests (116 unit + 13 contract + 9 E2E)

### Technical Decisions
- Playwright for E2E testing (browser automation)
- TypeScript strict mode for type safety
- Component-based error handling

---

## Architecture Summary

**Style**: Clean Architecture (Hexagonal) with CQRS-lite
**Patterns**: Strategy (processor selection), Pipeline (parsing flow), Repository (data access)
**Security**: XXE prevention, API key auth, CORS, input validation
**Testing**: 138 tests (307% of 45-test target)
**Documentation**: 10 ADRs, PRD v0.3, requirements traceability

**See**: `project-context/adr/` for architectural decision records
```

---

#### B. Clean README.md

**Remove**:
- All references to "Claude Code", "AI-assisted development", "MCP servers"
- Task tracking workflow sections
- Slash commands for AADF commits
- Progress tracking scripts
- SerenaAIxMCP references

**Keep**:
- Quick start (running the app)
- Development commands (build, test, run)
- Architecture overview
- Technology stack
- Key verification points

**Edit Strategy**:
1. Replace "Using Claude Code/AI" with "Developed with..."
2. Remove `/aadf-commit` slash command sections
3. Remove progress tracking workflow
4. Remove MCP server documentation
5. Keep architecture, testing, and getting started sections

**Example Before/After**:

**BEFORE**:
```markdown
## Development Methodology

### TDD/BDD Approach
...

### Code Principles
...

### MCP Servers & Tools

This project uses **Model Context Protocol (MCP) servers** for enhanced development capabilities:
...
```

**AFTER**:
```markdown
## Development Methodology

### TDD/BDD Approach
- **Prioritize happy path first**, then core edge cases
- Use xUnit + FluentAssertions (.NET), Playwright (React E2E)
- 138 tests: 116 unit, 13 contract, 9 E2E (307% of target)

### Code Principles
- SOLID, DRY, KISS, YAGNI
- Clean Architecture with CQRS-lite
- Type safety: aligned database schema, domain models, DTOs, TypeScript interfaces
```

---

### Phase 4: Update Main Branch (5 min)

```bash
# Merge submission into main
git checkout main
git merge submission --no-ff -m "chore: update main from submission branch

Submission-ready version with AI/process artifacts removed.

Changes:
- Remove AI orchestration config (.claude/, CLAUDE.md)
- Remove process tracking (tasks.json, PROGRESS.md, scripts)
- Condense documentation to essential artifacts
- Retain all application code, tests, and ADRs

See submission branch for details."

# Push to GitHub
git push origin main
```

---

### Phase 5: Final Verification (10 min)

```bash
# On submission branch
git checkout submission

# Verify build works
export PATH="$HOME/.dotnet:$PATH"
dotnet build
dotnet test

# Verify frontend builds
cd client
npm install
npm run build
cd ..

# Check what's left in repo
ls -la
du -sh project-context/*  # Should be minimal

# Verify README doesn't reference AI
grep -i "claude\|mcp\|serena\|aadf" README.md  # Should be empty

# Verify BUILDLOG is condensed
wc -l project-context/build-logs/BUILDLOG.md  # Should be ~100 lines (not 356)

# Check submission branch is clean
git status  # Should be clean
```

---

## 📊 Before/After Comparison

### File Count Reduction

**BEFORE** (dev branch):
```
project-context/
├── adr/ (10 files)                    ✅ KEEP
├── agents/ (~20 files)                ❌ DELETE
├── archive/ (1 file)                  ❌ DELETE
├── build-logs/ (2 files)              ✅ KEEP (1 condensed)
├── codex/ (1 file)                    ❌ DELETE
├── implementation/ (6+ files)         ❌ DELETE
├── learnings/ (8+ files)              ❌ DELETE
├── planning/ (3 files)                ❌ DELETE
├── requirements-and-analysis/ (2)     ✅ KEEP
├── reviews/ (6+ files)                ❌ DELETE
└── specifications/ (1 file)           ✅ KEEP

.claude/ (19+ agents)                  ❌ DELETE
.serena/ (cache)                       ❌ DELETE
scripts/ (13 files)                    ✅ KEEP (4 essential only)
```

**AFTER** (submission branch):
```
project-context/
├── adr/ (10 ADRs)                     ✅
├── build-logs/ (BUILDLOG.md ~100 lines) ✅
├── requirements-and-analysis/         ✅
└── specifications/                    ✅

scripts/ (4 files: build, test, dev)   ✅
```

### Size Reduction Estimate

**BEFORE**: ~15 MB (excluding node_modules, bin, obj)
**AFTER**: ~5 MB (67% reduction)

---

## 🚀 Post-Interview Actions

### After Interview (Keep Private)

```bash
# Archive branch stays local until you decide to backup
git checkout archive/full-development-history

# Optional: Push to a private GitHub repo (create new private repo first)
git remote add private-backup https://github.com/adarsh/flowingly-technical-test-archive.git
git push private-backup archive/full-development-history

# Tag submission version
git checkout submission
git tag -a v1.0-submission -m "Version submitted to Flowingly (2025-10-07)"
git push origin v1.0-submission
```

### If You Want to Share Full Process Later

```bash
# Create a blog post branch with selected learnings
git checkout -b blog/ai-assisted-development
git checkout archive/full-development-history -- project-context/learnings/
git checkout archive/full-development-history -- CLAUDE.md
git commit -m "docs: extract AI workflow learnings for blog post"

# Write blog post using these artifacts
# Publish blog, then share branch publicly
```

---

## ✅ Submission Checklist

Before pushing to GitHub:

- [ ] Archive branch created: `archive/full-development-history`
- [ ] Submission branch created: `submission`
- [ ] All AI artifacts removed (.claude/, CLAUDE.md, .serena/, etc.)
- [ ] Process tracking removed (implementation/, planning/, learnings/)
- [ ] BUILDLOG.md condensed to ~100 lines (high-level milestones only)
- [ ] README.md cleaned (no AI references)
- [ ] Build works: `dotnet build && dotnet test` ✅
- [ ] Frontend builds: `npm run build` ✅
- [ ] All tests pass: 138 tests green ✅
- [ ] Git status clean on submission branch
- [ ] Main branch updated from submission
- [ ] GitHub remote points to correct repo
- [ ] Ready to push: `git push origin submission && git push origin main`

---

## 📝 Commit Message for Submission

```
chore: prepare submission branch for Flowingly technical test

Remove AI orchestration and process tracking artifacts to focus on
application code, architecture, and testing.

Removed:
- .claude/ directory (19 AI agents, settings)
- CLAUDE.md (AI workflow instructions)
- .serena/, .playwright-mcp/ (MCP server state)
- project-context/implementation/ (task tracking, progress metrics)
- project-context/planning/ (delivery plans, agent resourcing)
- project-context/learnings/ (implementation insights)
- project-context/reviews/ (code reviews, testing notes)
- scripts/update-progress.sh and task automation scripts

Retained:
- Complete application code (src/, client/, tests/)
- 10 ADRs documenting architectural decisions
- PRD v0.3 technical specification
- Original test brief with requirements
- Condensed BUILDLOG (high-level milestones)
- Essential build/test scripts
- 138 passing tests (116 unit, 13 contract, 9 E2E)

Full development history preserved in local archive branch.

Clean Architecture + TDD approach with 307% test coverage.
```

---

## 🎯 Final Directory Structure (Submission Branch)

```
flowingly-technical-test/
├── client/                           # React frontend
│   ├── src/
│   ├── e2e/                         # Playwright E2E tests
│   ├── package.json
│   └── tsconfig*.json
├── contracts/                        # Shared DTOs
├── fixtures/                         # Test data
├── project-context/
│   ├── adr/                         # 10 ADRs
│   ├── build-logs/
│   │   └── BUILDLOG.md              # Condensed (~100 lines)
│   ├── requirements-and-analysis/
│   │   └── Full Stack Engineer Test (Sen) V2.pdf
│   └── specifications/
│       └── prd-technical_spec.md
├── scripts/
│   ├── build.sh
│   ├── dev.sh
│   ├── dev.ps1
│   └── test.sh
├── src/                              # .NET backend
│   ├── Api/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
├── tests/                            # xUnit + integration tests
├── .gitignore
├── Flowingly.ParsingService.sln
├── README.md                         # Cleaned (no AI references)
└── TEST-BRIEF-COMPLIANCE.md

TOTAL: ~50 files (down from ~150+)
```

---

## ⚠️ Important Notes

1. **Archive branch is local-only** until you explicitly push it (after interview)
2. **Never delete the archive branch** — it's your full history backup
3. **Test everything after cleanup** — ensure build/test scripts still work
4. **README.md edits are manual** — AI references must be removed by hand
5. **BUILDLOG.md condensing is manual** — keep high-level summaries only
6. **Git tags help** — tag v1.0-submission for easy reference
7. **Push submission + main** — both should be identical for GitHub

---

**Estimated Time**: 45 minutes (including manual edits + verification)

**Result**: Professional submission repo showcasing code quality, architecture, and testing — without overwhelming reviewers with AI/process artifacts.

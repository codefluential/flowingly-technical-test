# Submission Cleanup Plan v2

**Purpose**: Prepare clean submission branches (dev/main) for recruiter review while preserving full development history in local-only `dev-full-local` branch.

**Based on**: `cleanup-for-submission-instructions.md` (simplified approach)

---

## ⚠️ CRITICAL: Branch Preservation

**IMPORTANT**: Before executing ANY cleanup steps, you MUST create and verify the `dev-full-local` archive branch:

```bash
# Current branch: dev (full development history with AI artifacts)
git checkout dev
git status  # MUST be clean before archiving

# Create dev-full-local archive (NEVER push to remote)
git checkout -b dev-full-local
git tag -a v1.0-full-dev-archive -m "Complete development history before cleanup"

# Verify archive created
git branch -vv | grep dev-full-local  # Should show no remote tracking
git log --oneline -5             # Verify commits present
ls -la .claude/                  # Verify AI artifacts present

# Return to dev for cleanup
git checkout dev
```

**What's preserved in `dev-full-local` branch:**
- ✅ All AI orchestration (.claude/, CLAUDE.md, .serena/, .playwright-mcp/)
- ✅ All process tracking (implementation/, planning/, learnings/, reviews/)
- ✅ All automation scripts (update-progress.sh, reindex-serena.sh, etc.)
- ✅ Complete development history and metadata
- ✅ Full context for post-interview reference

**This branch is LOCAL ONLY** — Never push to GitHub until after interview (optional private backup).

---

## 📋 Three-Branch Strategy

### Branch 1: `dev-full-local` (Local Only - Full History)
**Purpose**: Preserve EVERYTHING for post-interview reference
**Location**: Local only (never push to GitHub)
**Contents**: Current dev branch as-is (100% fidelity)

### Branch 2: `dev` (Clean for Submission)
**Purpose**: Primary submission branch with cleaned codebase
**Location**: Push to GitHub (main submission branch)
**Contents**: Code + essential docs only

### Branch 3: `main` (Clean for Submission)
**Purpose**: Mirror of dev branch (GitHub default)
**Location**: Push to GitHub
**Contents**: Same as dev branch

---

## 🗑️ Files & Folders to Remove

### 1. Hidden Directories (Project Root)
```bash
.claude/                    # AI agent configurations (19 agents)
.serena/                    # MCP server cache
.playwright-mcp/            # Playwright MCP state
```

### 2. Scripts Directory (`/scripts`)
**Remove these files:**
```bash
scripts/reindex-serena.sh
scripts/backfill-task-durations.sh
scripts/update-progress.sh
scripts/FIXES-SUMMARY.md
scripts/TEST-SIGNAL-TRAP.md
scripts/VALIDATION.md
```

**Keep these files:**
```bash
scripts/build.sh            # Essential build script
scripts/test.sh             # Essential test runner
scripts/dev.sh              # Essential dev server
scripts/dev.ps1             # Windows dev script
scripts/clean.sh            # Clean build artifacts
scripts/run-script.js       # May be needed - verify first
scripts/README.md           # Update after cleanup (see Phase 3)
```

### 3. Project Context Directory (`/project-context`)
**Remove these folders:**
```bash
project-context/agents/         # AI agent definitions
project-context/archive/        # Historical notes
project-context/build-logs/     # Process tracking logs
project-context/codex/          # Codex reviews
project-context/learnings/      # Implementation insights
project-context/reviews/        # Code review notes (including this file!)
```

**Keep these folders:**
```bash
project-context/adr/                          # ✅ 10 ADRs (essential)
project-context/specifications/               # ✅ PRD v0.3, test brief
project-context/requirements-and-analysis/    # ✅ Original requirements
project-context/implementation/               # ✅ Partial (see cleanup below)
project-context/planning/                     # ✅ Delivery plan (keep minimal)
```

### 4. Implementation Folder (`/project-context/implementation`)
**Remove:**
```bash
project-context/implementation/dashboard/              # Dashboard artifacts
project-context/implementation/ENVIRONMENT.md          # Environment setup notes
project-context/implementation/TRACKING-WORKFLOW.md    # Progress tracking docs
project-context/implementation/README.md               # Binary/corrupted file
project-context/implementation/M3_CHANGES_APPLIED.md   # Internal review notes
project-context/implementation/M3_CODE_REVIEW_FINDINGS.md  # Internal review
project-context/implementation/M3_EXECUTION_PLAN.md    # Internal planning
```

**Keep:**
```bash
project-context/implementation/PROGRESS.md             # Final status snapshot
project-context/implementation/SMOKE_TEST_REPORT.md    # Test validation
project-context/implementation/tasks/                  # Task definitions (optional - see note below)
```

### 5. Root Files
**Remove:**
```bash
CLAUDE.md                   # AI instructions (22KB)
```

**Keep (with edits in Phase 3):**
```bash
README.md                   # Update for reviewers (remove AI references)
TEST-BRIEF-COMPLIANCE.md    # Requirements traceability
```

### 6. GitHub Workflows (`.github/workflows/`)
**Review and keep minimal:**
```bash
.github/workflows/e2e-tests.yml    # ✅ Keep (shows CI/CD understanding)
```

**Remove if present:**
- Any workflows referencing MCP servers, AI tooling, or progress tracking

---

## 🔧 Step-by-Step Execution Plan

### Phase 1: Create Archive Branch (5 min)

```bash
# Ensure all work is committed
git status  # Should be clean

# Create local dev-full-local archive from current dev
git checkout dev
git checkout -b dev-full-local

# Tag for reference
git tag -a v1.0-full-dev-archive -m "Complete development history before submission cleanup (2025-10-07)"

# Verify branch exists (do NOT push to GitHub)
git branch -vv
git log --oneline -5

# Switch back to dev for cleanup
git checkout dev
```

---

### Phase 2: Execute Cleanup on Dev Branch (15 min)

```bash
# On dev branch
git checkout dev

# === REMOVE: Hidden directories ===
rm -rf .claude/
rm -rf .serena/
rm -rf .playwright-mcp/

# === REMOVE: Root files ===
rm -f CLAUDE.md

# === REMOVE: Scripts ===
cd scripts/
rm -f reindex-serena.sh
rm -f backfill-task-durations.sh
rm -f update-progress.sh
rm -f FIXES-SUMMARY.md
rm -f TEST-SIGNAL-TRAP.md
rm -f VALIDATION.md
cd ..

# === REMOVE: Project context folders ===
rm -rf project-context/agents/
rm -rf project-context/archive/
rm -rf project-context/build-logs/
rm -rf project-context/codex/
rm -rf project-context/learnings/
rm -rf project-context/reviews/

# === REMOVE: Implementation folder cleanup ===
rm -rf project-context/implementation/dashboard/
rm -f project-context/implementation/ENVIRONMENT.md
rm -f project-context/implementation/TRACKING-WORKFLOW.md
rm -f project-context/implementation/README.md
rm -f project-context/implementation/M3_CHANGES_APPLIED.md
rm -f project-context/implementation/M3_CODE_REVIEW_FINDINGS.md
rm -f project-context/implementation/M3_EXECUTION_PLAN.md

# === OPTIONAL: Remove tasks orchestration ===
# Uncomment if you want to remove task tracking entirely:
# rm -rf project-context/implementation/tasks/
# rm -f project-context/implementation/PROGRESS.md

# === Stage all deletions ===
git add -A

# === Commit cleanup ===
git commit -m "chore: prepare submission branch - remove AI/process artifacts

Remove AI orchestration, MCP infrastructure, and internal process tracking
to focus submission on application code, architecture, and testing.

Removed:
- .claude/, .serena/, .playwright-mcp/ (AI/MCP infrastructure)
- CLAUDE.md (AI workflow instructions)
- project-context/agents/, learnings/, reviews/, codex/ (internal docs)
- project-context/build-logs/, archive/ (process tracking)
- scripts/update-progress.sh, reindex-serena.sh, etc. (automation)
- project-context/implementation/ internal review files

Retained:
- All application code (src/, client/, tests/, contracts/, fixtures/)
- 10 ADRs documenting architectural decisions
- PRD v0.3 technical specification
- Original test brief with requirements
- Essential build/test/dev scripts
- 195 passing tests (118 backend + 77 E2E)
- Final progress snapshot and smoke test report

Full development history preserved in dev-full-local branch (local only)."
```

---

### Phase 3: Manual Documentation Edits (20 min)

#### A. Update `README.md`

**Remove all references to:**
- Claude Code / AI-assisted development
- MCP servers (Serena, Context7, Playwright MCP)
- Slash commands (`/aadf-commit`, `/aadf-commit-staged`)
- Progress tracking workflow (`update-progress.sh`)
- Task tracking system

**Sections to remove:**
1. "MCP Servers & Tools" (entire section ~lines 308-436)
2. "Slash Commands (AADF-Compliant Git Workflow)" (entire section)
3. Progress tracking references in "Development Commands"

**Sections to keep/update:**
1. ✅ **Quick Start** (verify paths work without MCP tools)
2. ✅ **Development Commands** (backend, frontend, testing)
3. ✅ **Architecture** (Clean Architecture explanation)
4. ✅ **Technology Stack**
5. ✅ **Key Verification Points**

**Replace references:**
- "Using Claude Code/MCP" → "Developed with TDD/BDD approach"
- Remove SerenaAIxMCP mentions
- Simplify to essential commands only

**Verification command:**
```bash
# After editing, verify no AI references remain
grep -i "claude\|mcp\|serena\|aadf\|ai-assisted\|model context protocol" README.md
# Should return empty (or only in Git Co-Authored-By footer if kept in examples)
```

#### B. Update `scripts/README.md`

Create new `scripts/README.md` (or update if exists):

```markdown
# Development Scripts

Essential build, test, and development automation scripts.

## Available Scripts

### Build & Test
- **`build.sh`** — Build the .NET solution and frontend
- **`test.sh`** — Run all tests (backend unit/integration/contract + E2E)
- **`clean.sh`** — Clean build artifacts

### Development Servers
- **`dev.sh`** — Start both backend API and frontend dev server (Linux/Mac)
- **`dev.ps1`** — Start both backend API and frontend dev server (Windows)

## Usage

```bash
# Build entire project
./scripts/build.sh

# Run all tests
./scripts/test.sh

# Start development servers
./scripts/dev.sh  # Linux/Mac
# OR
powershell ./scripts/dev.ps1  # Windows

# Clean build artifacts
./scripts/clean.sh
```

## Prerequisites

- .NET 8 SDK (8.0.414+)
- Node.js 18+
- Playwright browsers installed (`npx playwright install`)

## Notes

- Backend API runs on http://localhost:5000
- Frontend runs on http://localhost:5173
- Swagger documentation at http://localhost:5000/swagger
```

#### C. Update `project-context/README.md`

**Simplify to essential sections only:**

```markdown
# Project Context Documentation

This directory contains comprehensive documentation for the **Flowingly Parsing Service** project.

## 📁 Documentation Structure

### 📋 Specifications (`/specifications`)
- **[prd-technical_spec.md](specifications/prd-technical_spec.md)** — PRD v0.3 (current)
  - Complete product requirements and technical design
  - API contracts, validation rules, error handling

### 🏗️ Architecture Decision Records (`/adr`)
**10 ADRs documenting key architectural decisions:**
- ADR-0001: Storage choice (SQLite dev, Postgres production)
- ADR-0002: Clean Architecture + CQRS-lite
- ADR-0003: Processor Strategy pattern
- ADR-0004: Swagger API documentation
- ADR-0005: URI-based versioning
- ADR-0006: API key authentication
- ADR-0007: Response contract design (XOR structure)
- ADR-0008: Parsing & validation rules
- ADR-0009: Banker's Rounding policy
- ADR-0010: Test strategy & coverage

See **[adr/README.md](adr/README.md)** for complete index.

### 📊 Requirements & Analysis (`/requirements-and-analysis`)
- **Full Stack Engineer Test (Sen) V2.pdf** — Original test brief
- Requirements analysis and compliance mapping

### 📝 Implementation (`/implementation`)
- **PROGRESS.md** — Final project status (51/51 tasks, 195 tests)
- **SMOKE_TEST_REPORT.md** — Production readiness validation

### 📅 Planning (`/planning`)
- **delivery-plan.md** — Milestone-based delivery roadmap (M0–M3)

---

## 🎯 Quick Navigation

**Starting Review:**
1. Read [prd-technical_spec.md](specifications/prd-technical_spec.md) (requirements)
2. Review [adr/README.md](adr/README.md) (architectural decisions)
3. Check [implementation/PROGRESS.md](implementation/PROGRESS.md) (completion status)

**Understanding Architecture:**
- ADR-0002: Architecture pattern
- ADR-0003: Content routing strategy
- ADR-0007: API response design

**Test Coverage:**
- ADR-0010: Test strategy (195 tests: 118 backend + 77 E2E)
- implementation/SMOKE_TEST_REPORT.md

---

## 🔑 Key Engineering Decisions

| Decision | Value | Reference |
|----------|-------|-----------|
| **Rounding Policy** | Banker's Rounding (MidpointRounding.ToEven) | ADR-0009 |
| **Tag Validation** | Stack-based nesting validation | ADR-0008 |
| **Response Contract** | Classification-specific (expense XOR other) | ADR-0007 |
| **Database** | SQLite local/test, Postgres production | ADR-0001 |
| **Architecture** | Clean/Hexagonal + CQRS-lite | ADR-0002 |
| **API Versioning** | URI-based (`/api/v1/`) | ADR-0005 |

---

**Project Status:** ✅ Phase 1 Complete (M0–M3), Production Ready
**Test Coverage:** 195 passing tests (433% of 45+ target)
**Last Updated:** 2025-10-07
```

#### D. Commit Documentation Updates

```bash
git add README.md scripts/README.md project-context/README.md
git commit -m "docs: update documentation for submission

Remove AI/MCP workflow references and simplify for reviewer clarity.

Changes:
- README.md: Remove MCP servers, slash commands, progress tracking sections
- scripts/README.md: Focus on essential build/test/dev scripts
- project-context/README.md: Simplify to essential navigation

Focus on application functionality, architecture, and testing approach."
```

---

### Phase 4: Update Main Branch (5 min)

```bash
# Merge dev into main
git checkout main
git merge dev --ff-only -m "chore: sync main with cleaned dev branch for submission"

# Verify main and dev are identical
git diff dev main  # Should be empty

# Push both branches to GitHub
git push origin dev
git push origin main
```

---

### Phase 5: Final Verification (10 min)

```bash
# On dev branch
git checkout dev

# === Verify builds work ===
export PATH="$HOME/.dotnet:$PATH"
dotnet clean
dotnet build
dotnet test

# === Verify frontend builds ===
cd client
npm install  # Should work without root package.json
npm run build
npm run test:e2e  # Run quick smoke test
cd ..

# === Check directory structure ===
ls -la | grep "^\."  # Should only see .git, .github, .gitignore
du -sh project-context/*  # Should be minimal

# === Verify no AI references in README ===
grep -i "claude\|mcp\|serena\|aadf" README.md  # Should be clean

# === Verify git status ===
git status  # Should be clean

# === Check file counts ===
find project-context -type f | wc -l  # Should be ~30-40 files (down from 150+)

# === Verify branches ===
git branch -vv
# Should show:
#   dev (tracking origin/dev)
#   dev-full-local (no remote)
#   main (tracking origin/main)
```

---

## 📊 Before/After Comparison

### Directory Structure

**BEFORE (dev-full-local branch):**
```
project-context/
├── adr/ (10 files)
├── agents/ (~20 files) ❌
├── archive/ (1 file) ❌
├── build-logs/ (2 files) ❌
├── codex/ (1 file) ❌
├── implementation/ (15+ files) ❌ (most)
├── learnings/ (8+ files) ❌
├── planning/ (3 files) ✅
├── requirements-and-analysis/ (2) ✅
├── reviews/ (6+ files) ❌
└── specifications/ (1 file) ✅

.claude/ (19+ agents) ❌
.serena/ (cache) ❌
.playwright-mcp/ (state) ❌
scripts/ (13 files) ✅ (7 kept)
CLAUDE.md ❌
```

**AFTER (dev/main branches):**
```
project-context/
├── adr/ (10 ADRs) ✅
├── implementation/
│   ├── PROGRESS.md ✅
│   ├── SMOKE_TEST_REPORT.md ✅
│   └── tasks/ (optional) ✅
├── planning/ (delivery-plan.md) ✅
├── requirements-and-analysis/ ✅
└── specifications/ ✅

.github/ (workflows) ✅
scripts/ (7 files) ✅
README.md (cleaned) ✅
TEST-BRIEF-COMPLIANCE.md ✅
```

### Size Reduction

**BEFORE**: ~15 MB (excluding node_modules, bin, obj)
**AFTER**: ~5 MB (67% reduction)
**Files**: ~150+ → ~40 files in project-context/

---

## ✅ Submission Checklist

Before pushing to GitHub:

- [ ] Archive branch created: `dev-full-local` ✅
- [ ] Archive branch verified: `git log dev-full-local --oneline -5` ✅
- [ ] Archive tagged: `v1.0-full-dev-archive` ✅
- [ ] Dev branch cleaned: All AI artifacts removed ✅
- [ ] README.md updated: No AI/MCP references ✅
- [ ] scripts/README.md updated ✅
- [ ] project-context/README.md simplified ✅
- [ ] Build works: `dotnet build && dotnet test` ✅
- [ ] Frontend builds: `cd client && npm run build` ✅
- [ ] E2E tests pass: `npm run test:e2e` ✅
- [ ] Git status clean on dev ✅
- [ ] Main branch updated from dev ✅
- [ ] Dev and main identical: `git diff dev main` (empty) ✅
- [ ] Ready to push: `git push origin dev && git push origin main` ✅

---

## 🚀 Post-Cleanup Verification Commands

```bash
# Switch to dev branch
git checkout dev

# Verify no AI references in README
grep -i "claude\|mcp\|serena\|aadf" README.md

# Verify hidden directories removed
ls -la | grep "^\." | grep -v ".git\|.github\|.gitignore"
# Should only show .git, .github, .gitignore

# Verify project-context is minimal
find project-context -type d
# Should only show: adr, implementation, planning, requirements-and-analysis, specifications

# Verify builds and tests work
export PATH="$HOME/.dotnet:$PATH"
dotnet clean && dotnet build && dotnet test
cd client && npm run build && cd ..

# Verify branches
git branch -vv
# dev-full-local (no remote)
# dev (tracking origin/dev)
# main (tracking origin/main)

# Verify dev-full-local has full history
git checkout dev-full-local
ls -la .claude/  # Should exist
git checkout dev
ls -la .claude/  # Should NOT exist
```

---

## ⚠️ Important Notes

1. **dev-full-local branch is local-only** — Never push to GitHub (contains AI artifacts)
2. **Archive before cleanup** — Verify `dev-full-local` branch exists before deleting files
3. **Test after cleanup** — Ensure build/test scripts work without MCP dependencies
4. **Manual edits required** — README.md and project-context/README.md need hand editing
5. **Dev and main should match** — Both submission branches identical for GitHub
6. **Keep `.github/workflows/`** — Shows CI/CD understanding (remove if AI-specific)
7. **Backup dev-full-local** — Consider pushing to private repo after interview

---

## 🎯 What Recruiters Will See

**On GitHub (dev/main branches):**
- ✅ Clean, professional codebase
- ✅ Working application (195 tests passing)
- ✅ Clear architecture (10 ADRs)
- ✅ Requirements traceability
- ✅ Build/test/dev scripts
- ✅ No AI/process tracking clutter

**What they WON'T see:**
- ❌ AI orchestration (.claude/, CLAUDE.md)
- ❌ MCP server infrastructure
- ❌ Internal process tracking
- ❌ Task duration metrics
- ❌ Code review notes
- ❌ Implementation planning artifacts

---

**Estimated Time**: 45-60 minutes (including manual edits + verification)

**Result**: Professional submission showcasing code quality, architecture, testing, and SDLC understanding — without overwhelming reviewers with AI/process artifacts.

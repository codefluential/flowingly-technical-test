# Development Scripts

This directory contains automation scripts for the Flowingly Parsing Service development workflow.

## Quick Reference

| Script | Purpose | Platform | Usage |
|--------|---------|----------|-------|
| `update-progress.sh` | Task progress tracking | Unix/Linux/macOS/WSL/Git Bash | `./scripts/update-progress.sh task_001 completed` |
| `reindex-serena.sh` | Serena MCP semantic indexing | Unix/Linux/macOS/WSL/Git Bash | `./scripts/reindex-serena.sh` |
| `build.sh` | Build .NET solution | Unix/Linux/macOS/WSL/Git Bash | `./scripts/build.sh` |
| `test.sh` | Run all tests | Unix/Linux/macOS/WSL/Git Bash | `./scripts/test.sh` |
| `clean.sh` | Clean build artifacts | Unix/Linux/macOS/WSL/Git Bash | `./scripts/clean.sh` |
| `dev.sh` | Start dev environment | Unix/Linux/macOS/WSL/Git Bash | `./scripts/dev.sh` |
| `dev.ps1` | Start dev environment | Windows PowerShell | `.\scripts\dev.ps1` |
| `backfill-task-durations.sh` | Calculate task durations | Unix/Linux/macOS/WSL/Git Bash | `./scripts/backfill-task-durations.sh` |

---

## Core Scripts

### `update-progress.sh` - Task Progress Tracking

**Purpose**: Update task status, sync progress tracking files, and trigger semantic re-indexing.

**Features**:
- Updates `tasks.json` with task status and timestamps
- Calculates task duration automatically (started_at ’ completed_at)
- Updates `PROGRESS.md` checkboxes and metrics
- Appends to `BUILDLOG.md` for milestone gates (task_010, 030, 040, 050)
- Triggers background Serena re-indexing for semantic code analysis
- Suggests conventional commit messages

**Usage**:
```bash
# Mark task as in-progress
./scripts/update-progress.sh task_001 in_progress

# Mark task as completed
./scripts/update-progress.sh task_001 completed

# Complete with test counts
./scripts/update-progress.sh task_014 completed unit 7
./scripts/update-progress.sh task_037 completed contract 13
./scripts/update-progress.sh task_045 completed e2e 9

# Block a task
./scripts/update-progress.sh task_015 blocked
```

**Status Values**:
- `pending` - Not yet started
- `in_progress` - Currently working on
- `completed` - Finished successfully
- `blocked` - Cannot proceed due to dependency/issue

**Test Type Arguments** (optional, for completed tasks):
- `unit` - Unit test count
- `contract` - Contract/integration test count
- `e2e` - End-to-end test count
- `smoke` - Smoke test count

**Duration Tracking**:
- `started_at`: ISO timestamp when marked `in_progress`
- `completed_at`: ISO timestamp when marked `completed`
- `duration_minutes`: Auto-calculated on completion (rounded to nearest minute)

**Automatic Actions**:
- **Milestone Gates** (task_010, 030, 040, 050): Appends entry to `BUILDLOG.md`
- **Serena Re-indexing**: Triggers background semantic indexing (non-blocking)
- **Progress Dashboard**: Updates `PROGRESS.md` with real-time metrics

**Related Documentation**:
- `project-context/implementation/TRACKING-WORKFLOW.md` - Complete workflow guide
- `project-context/implementation/PROGRESS.md` - Progress dashboard
- `project-context/build-logs/BUILDLOG.md` - Milestone history

---

### `reindex-serena.sh` - Serena MCP Semantic Indexing

**Purpose**: Manually trigger Serena MCP semantic code analysis re-indexing.

**When to Use**:
-  After adding new classes, methods, or interfaces
-  After renaming symbols (classes, methods, properties)
-  After major refactoring
-  When semantic search isn't finding new code

**When NOT Needed**:
- L After editing method bodies only
- L After changing strings or comments
- L After running tests
- L After updating documentation

**Usage**:
```bash
# Foreground indexing (blocks until complete)
./scripts/reindex-serena.sh
```

**Performance**: Typically completes in 10-30 seconds.

**Note**: `update-progress.sh` triggers background re-indexing automatically when completing tasks. Manual re-indexing is only needed when working outside the task system.

**Related Documentation**:
- `project-context/learnings/serena-mcp-guide.md` - Serena MCP comprehensive guide
- `.serena/project.yml` - Serena configuration

---

### `build.sh` - Build Solution

**Purpose**: Clean build of entire .NET solution.

**Usage**:
```bash
./scripts/build.sh
```

**Equivalent Command**:
```bash
dotnet clean
dotnet build
```

**Exit Codes**:
- `0` - Build successful
- `1` - Build failed

---

### `test.sh` - Run All Tests

**Purpose**: Execute all unit tests, contract tests, and E2E tests.

**Usage**:
```bash
./scripts/test.sh
```

**Test Coverage** (Current):
- 116 unit tests (Domain/Application logic)
- 13 contract tests (API integration)
- 21 E2E tests (Playwright browser automation)
- **Total: 150 tests**

**Equivalent Commands**:
```bash
# Backend tests (unit + contract)
dotnet test

# Frontend E2E tests
cd client && npm run test:e2e
```

**Exit Codes**:
- `0` - All tests passed
- `1` - One or more tests failed

---

### `clean.sh` - Clean Build Artifacts

**Purpose**: Remove all build artifacts, bin/obj directories, and temporary files.

**Usage**:
```bash
./scripts/clean.sh
```

**What Gets Cleaned**:
- `bin/` directories
- `obj/` directories
- `.dotnet/` temporary files
- NuGet package cache (optional)

**Equivalent Command**:
```bash
dotnet clean
```

---

### `dev.sh` / `dev.ps1` - Start Development Environment

**Purpose**: Launch both backend API and frontend dev server in parallel.

**Unix/Linux/macOS/WSL/Git Bash**:
```bash
./scripts/dev.sh
```

**Windows PowerShell**:
```powershell
.\scripts\dev.ps1
```

**What It Does**:
1. Starts backend API at `http://localhost:5000` (with hot reload)
2. Starts frontend Vite dev server at `http://localhost:5173`
3. Opens browser to frontend URL (dev.ps1 only)

**Manual Alternative**:
```bash
# Terminal 1
dotnet watch --project src/Api

# Terminal 2
cd client && npm run dev
```

**Stop Development Environment**:
- Press `Ctrl+C` in the terminal running the script
- Both servers will shut down

---

### `backfill-task-durations.sh` - Calculate Task Durations

**Purpose**: Retroactively calculate `duration_minutes` for tasks that have `started_at` and `completed_at` timestamps but missing duration.

**Usage**:
```bash
./scripts/backfill-task-durations.sh
```

**What It Does**:
- Reads `tasks.json`
- Finds tasks with `started_at` and `completed_at` but no `duration_minutes`
- Calculates duration in minutes (rounded)
- Updates `tasks.json` with calculated durations

**Use Cases**:
- After manually editing `tasks.json`
- After importing tasks from another system
- Fixing missing duration data

**Output**:
```
Backfilling task durations...
Updated task_001: 45 minutes
Updated task_002: 120 minutes
Updated task_003: 30 minutes
Total tasks updated: 3
```

---

## Platform Compatibility

### Unix/Linux/macOS/WSL/Git Bash Scripts (`.sh`)

**Requirements**:
- Bash shell
- Unix tools: `jq`, `grep`, `sed`, `awk`

**Platforms**:
-  Linux (all distributions)
-  macOS (all versions)
-  Windows Subsystem for Linux (WSL)
-  Git Bash for Windows

**Installation (if missing tools)**:
```bash
# Ubuntu/Debian
sudo apt-get install jq

# macOS (Homebrew)
brew install jq

# Fedora/RHEL
sudo dnf install jq
```

### Windows PowerShell Scripts (`.ps1`)

**Requirements**:
- PowerShell 5.1+ (Windows) or PowerShell 7+ (cross-platform)

**Platforms**:
-  Windows 10/11 (built-in)
-  Windows Server 2016+ (built-in)

**Execution Policy** (if script won't run):
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

---

## Windows Users - Script Options

### Option 1: Git Bash (Recommended)

**Advantages**:
-  All `.sh` scripts work identically to Linux/macOS
-  Unix command compatibility
-  Familiar bash syntax

**Installation**:
1. Download Git for Windows: https://git-scm.com/download/win
2. During install, select "Git Bash Here" context menu option
3. Launch via Start Menu ’ "Git Bash"

**Usage**:
```bash
./scripts/update-progress.sh task_001 completed
```

### Option 2: WSL (Windows Subsystem for Linux)

**Advantages**:
-  Full Linux environment in Windows
-  Native bash and Unix tools

**Installation**:
```powershell
wsl --install
```

**Usage**:
```bash
./scripts/update-progress.sh task_001 completed
```

### Option 3: PowerShell Scripts

**Advantages**:
-  Native Windows experience
-  No additional software required

**Usage**:
```powershell
.\scripts\dev.ps1
```

**Note**: Only `dev.ps1` currently available. For other scripts, use Git Bash or WSL.

### Option 4: Manual Execution

**For any script**:
1. Open script file in text editor
2. Read commands inside
3. Execute commands manually in PowerShell/cmd

---

## Development Workflow Integration

### Typical Development Session

```bash
# 1. Start development environment
./scripts/dev.sh

# 2. Start a task
./scripts/update-progress.sh task_042 in_progress

# 3. Write code, run tests
dotnet test --filter "FullyQualifiedName~MyNewFeature"

# 4. Complete task with test count
./scripts/update-progress.sh task_042 completed unit 5

# 5. Commit changes
git add .
git commit -m "feat(feature): implement new feature with tests"

# 6. Continue to next task
./scripts/update-progress.sh task_043 in_progress
```

### After Major Refactoring

```bash
# 1. Complete task
./scripts/update-progress.sh task_XXX completed

# 2. Manually re-index if needed (usually automatic)
./scripts/reindex-serena.sh

# 3. Verify semantic search
# Use Serena MCP tools to find refactored symbols
```

---

## Troubleshooting

### Permission Denied

**Problem**: `bash: ./scripts/build.sh: Permission denied`

**Solution**:
```bash
chmod +x scripts/*.sh
```

### Script Not Found

**Problem**: `bash: ./scripts/update-progress.sh: No such file or directory`

**Solution**:
1. Verify you're in project root: `pwd` should show `.../flowingly-technical-test`
2. Check file exists: `ls scripts/update-progress.sh`
3. Use absolute path if needed: `/path/to/flowingly-technical-test/scripts/update-progress.sh`

### jq Command Not Found

**Problem**: `bash: jq: command not found`

**Solution**:
```bash
# Ubuntu/Debian
sudo apt-get install jq

# macOS
brew install jq

# Fedora/RHEL
sudo dnf install jq
```

### PowerShell Execution Policy Error

**Problem**: `cannot be loaded because running scripts is disabled`

**Solution**:
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

### Serena Re-indexing Hangs

**Problem**: `reindex-serena.sh` runs forever

**Solution**:
1. Press `Ctrl+C` to cancel
2. Check `.serena/cache/` directory size: `du -sh .serena/cache/`
3. If >500MB, clear cache: `rm -rf .serena/cache/*`
4. Re-run: `./scripts/reindex-serena.sh`

---

## Script Maintenance

### Adding New Scripts

1. Create script file: `scripts/my-new-script.sh`
2. Add shebang: `#!/bin/bash`
3. Make executable: `chmod +x scripts/my-new-script.sh`
4. Document in this README
5. Test on all platforms (Linux, macOS, Git Bash)

### Bash Script Best Practices

```bash
#!/bin/bash
set -e  # Exit on error
set -u  # Exit on undefined variable
set -o pipefail  # Exit on pipe failure

# Clear error messages
# Descriptive variable names
# Comment complex logic
# Validate inputs
```

### PowerShell Script Best Practices

```powershell
#Requires -Version 5.1
$ErrorActionPreference = "Stop"

# Use approved verbs (Get-, Set-, New-, etc.)
# Parameter validation
# Clear error messages
# Comment complex logic
```

---

## Related Documentation

- **Main README**: `../README.md` - Project overview and quick start
- **Progress Tracking**: `../project-context/implementation/TRACKING-WORKFLOW.md`
- **Serena MCP Guide**: `../project-context/learnings/serena-mcp-guide.md`
- **Build History**: `../project-context/build-logs/BUILDLOG.md`
- **Task System**: `../project-context/implementation/tasks/`

---

## Support

For issues or questions:
1. Check troubleshooting section above
2. Review related documentation
3. Inspect script contents (they're readable shell scripts)
4. Check `VALIDATION.md` for common fixes

**Happy scripting!** =€

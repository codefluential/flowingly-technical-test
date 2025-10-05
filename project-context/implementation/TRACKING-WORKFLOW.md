# Progress Tracking Workflow

**Purpose**: Integrated tracking system for task execution, milestone completion, and test progress.

---

## Overview

The tracking system consists of **3 interconnected components**:

1. **PROGRESS.md** - Human-readable progress dashboard
2. **tasks.json** - Machine-readable task status and metrics
3. **BUILDLOG.md** - Historical log of milestones and changes

All three stay synchronized automatically via the `update-progress.sh` script.

---

## Quick Start

### When Starting a Task
```bash
# Mark task as in-progress
./scripts/update-progress.sh task_001 in_progress

# This updates:
# - tasks.json: status="in_progress", started_at timestamp
# - PROGRESS.md: current task indicator
# - TodoWrite tool: active task tracking
```

### When Completing a Task
```bash
# Mark task as completed
./scripts/update-progress.sh task_001 completed

# With test counts (if tests added):
./scripts/update-progress.sh task_014 completed unit 7

# This updates:
# - tasks.json: status="completed", completed_at timestamp, counters
# - PROGRESS.md: checkbox marked, progress percentage
# - BUILDLOG.md: milestone entry (if DoD task)
```

### When Blocked
```bash
# Mark task as blocked
./scripts/update-progress.sh task_015 blocked

# Document blocker in PROGRESS.md "Blockers & Issues" section manually
```

---

## The Three Tracking Files

### 1. PROGRESS.md (Dashboard)
**Location**: `project-context/implementation/PROGRESS.md`

**Purpose**: Quick visual status for humans

**Contains**:
- Current milestone and task
- Task completion checkboxes per milestone
- Test suite status (unit, contract, E2E)
- Blockers and issues
- Next actions

**Updates**: Automatically by script + manual for blockers

### 2. tasks.json (Machine State)
**Location**: `project-context/implementation/tasks/tasks.json`

**Purpose**: Programmatic tracking and metrics

**Contains**:
```json
{
  "progress_tracking": {
    "last_updated": "2025-10-06T03:17:00Z",
    "current_milestone": "M0",
    "current_task": "task_001",
    "tasks_completed": 0,
    "tests_passing": {
      "unit": 0,
      "contract": 0,
      "e2e": 0
    },
    "milestones_completed": []
  },
  "tasks": [
    {
      "id": "task_001",
      "status": "pending",  // pending|in_progress|completed|blocked
      "started_at": null,
      "completed_at": null
    }
  ]
}
```

**Updates**: Automatically by script

### 3. BUILDLOG.md (History)
**Location**: `project-context/build-logs/BUILDLOG.md`

**Purpose**: Historical record of milestones and major changes

**Contains**:
- Chronological log (oldest first)
- Milestone completion entries (M0, M1, M2, M3)
- Major changes and decisions
- Test progress at each milestone

**Updates**: Automatically for milestones, manual for other changes

---

## Workflow Integration

### With Git Commits

**After completing a task**:
```bash
# 1. Update progress
./scripts/update-progress.sh task_001 completed

# 2. Script suggests commit message
# Output: "task_001: Create Solution Structure"

# 3. Commit with suggested message
git add .
git commit -m "feat(scaffold): create .NET solution structure (task_001)

- Created Flowingly.ParsingService.sln with Clean Architecture layers
- Configured Api, Application, Domain, Infrastructure projects
- Setup project references and dependencies
- Added initial solution structure following ADR-0002

Progress: 1/50 tasks (2%)

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"

# 4. Commit progress files
git add project-context/implementation/tasks/tasks.json \
        project-context/implementation/PROGRESS.md
git commit -m "chore(progress): update task_001 status to completed

Progress: 1/50 tasks complete

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

### With TodoWrite Tool

**During a session**, use TodoWrite to track current work:

```bash
# Session starts
TodoWrite: [
  {"content": "Execute task_001: Create Solution Structure", "status": "in_progress"},
  {"content": "Verify task_001 acceptance criteria", "status": "pending"}
]

# Task completes
TodoWrite: [
  {"content": "Execute task_001: Create Solution Structure", "status": "completed"},
  {"content": "Verify task_001 acceptance criteria", "status": "completed"},
  {"content": "Update progress tracking", "status": "in_progress"}
]

# Then run:
./scripts/update-progress.sh task_001 completed
```

### With BUILDLOG Updates

**Milestone completions auto-update BUILDLOG**:

```bash
# Complete M0 DoD task
./scripts/update-progress.sh task_010 completed

# Script automatically appends to BUILDLOG.md:
## 2025-10-06 12:00 - M0 Milestone Complete

**Changes**:
- Completed all M0 tasks successfully
- M0 DoD verification passed (task_010)

**Testing**:
- Tests passing: 0/45

**Next Steps**:
- Proceed to task_011
```

---

## Task Status Lifecycle

```
pending → in_progress → completed
                    ↓
                  blocked (→ in_progress when unblocked)
```

### Status Definitions

- **pending**: Not started, waiting for dependencies
- **in_progress**: Currently being worked on
- **completed**: Done and verified
- **blocked**: Cannot proceed due to blocker (document in PROGRESS.md)

---

## Milestone Gates (DoD Tasks)

**Special tracking for milestone verification tasks**:

| Task | Milestone | What It Does |
|------|-----------|--------------|
| task_010 | M0 DoD | Verifies M0 complete, auto-updates BUILDLOG |
| task_030 | M1 DoD | Verifies M1 complete, auto-updates BUILDLOG |
| task_040 | M2 DoD | Verifies M2 complete, auto-updates BUILDLOG |
| task_050 | M3 DoD | Verifies M3 complete, marks SUBMITTABLE |

**When completing a DoD task**:
1. Script updates `tasks.json` → `milestones_completed` array
2. Script appends milestone entry to `BUILDLOG.md`
3. Script updates `PROGRESS.md` milestone section
4. **Manual**: Create git commit with milestone summary

---

## Test Progress Tracking

**When tests are added/passing**:

```bash
# After writing unit tests (TDD RED phase)
./scripts/update-progress.sh task_014 completed unit 7
# Adds 7 to unit test count

# After E2E tests pass
./scripts/update-progress.sh task_045 completed e2e 3
# Adds 3 to E2E test count

# Check test status
cat project-context/implementation/tasks/tasks.json | jq '.progress_tracking.tests_passing'
```

**Test count targets**:
- Unit: 30+ tests (M1)
- Contract: 10+ tests (M2)
- E2E: 5+ tests (M3)
- **Total: 45+ tests** (M3 DoD requirement)

---

## Query Progress (JQ Examples)

```bash
# Current status
jq '.progress_tracking' project-context/implementation/tasks/tasks.json

# Tasks in progress
jq '.tasks[] | select(.status == "in_progress")' project-context/implementation/tasks/tasks.json

# Completed tasks count
jq '[.tasks[] | select(.status == "completed")] | length' project-context/implementation/tasks/tasks.json

# Blocked tasks
jq '.tasks[] | select(.status == "blocked") | {id, name}' project-context/implementation/tasks/tasks.json

# Milestone progress
jq '.milestones_completed' project-context/implementation/tasks/tasks.json

# Test progress
jq '.progress_tracking.tests_passing' project-context/implementation/tasks/tasks.json
```

---

## Complete Workflow Example

### Scenario: Completing task_001 (Create Solution Structure)

```bash
# 1. Start task (updates tasks.json, PROGRESS.md)
./scripts/update-progress.sh task_001 in_progress

# 2. Use TodoWrite during work
TodoWrite: [
  {"content": "Create .NET solution with Clean Architecture", "status": "in_progress"},
  {"content": "Configure project references", "status": "pending"}
]

# 3. Do the work (create solution, projects, etc.)

# 4. Complete TodoWrite items
TodoWrite: [
  {"content": "Create .NET solution with Clean Architecture", "status": "completed"},
  {"content": "Configure project references", "status": "completed"}
]

# 5. Mark task complete (updates tasks.json, PROGRESS.md)
./scripts/update-progress.sh task_001 completed
# Output: ✅ Progress updated: task_001 → completed
#         Tasks: 1/50 (2%)
#         📝 Suggested commit: task_001: Create Solution Structure

# 6. Commit the work
git add .
git commit -m "feat(scaffold): create .NET solution structure (task_001)

[implementation details]

Progress: 1/50 tasks (2%)
..."

# 7. Commit progress files
git add project-context/implementation/tasks/tasks.json \
        project-context/implementation/PROGRESS.md
git commit -m "chore(progress): update task_001 status to completed

Progress: 1/50 tasks complete
..."

# 8. Move to next task
./scripts/update-progress.sh task_002 in_progress
```

---

## Troubleshooting

### Script Fails
```bash
# Check jq is installed
which jq

# Install if missing
sudo apt-get install jq  # Debian/Ubuntu
```

### Progress Out of Sync
```bash
# Manually edit tasks.json to fix status
# Then regenerate PROGRESS.md from tasks.json

# Or revert and re-run
git restore project-context/implementation/PROGRESS.md
./scripts/update-progress.sh <task_id> <status>
```

### Merge Conflicts
- **tasks.json**: Auto-merge will fail, manually resolve
- **PROGRESS.md**: Can usually auto-merge
- **BUILDLOG.md**: Appends only, rarely conflicts

---

## Best Practices

✅ **DO**:
- Run `update-progress.sh` after every task completion
- Use TodoWrite for session-level tracking
- Commit progress files separately from implementation
- Document blockers immediately in PROGRESS.md
- Verify DoD criteria before marking gates complete

❌ **DON'T**:
- Manually edit tasks.json (use script)
- Skip progress updates (breaks tracking)
- Batch multiple task updates (do one at a time)
- Forget to commit progress files

---

## Quick Reference

### Key Files
- `project-context/implementation/PROGRESS.md` - Dashboard
- `project-context/implementation/tasks/tasks.json` - State
- `project-context/build-logs/BUILDLOG.md` - History
- `scripts/update-progress.sh` - Automation

### Key Commands
```bash
# Start task
./scripts/update-progress.sh <task_id> in_progress

# Complete task
./scripts/update-progress.sh <task_id> completed [test_type] [count]

# Block task
./scripts/update-progress.sh <task_id> blocked

# Query status
jq '.progress_tracking' project-context/implementation/tasks/tasks.json
```

---

**Last Updated**: 2025-10-06
**Status**: Tracking system ready for Phase 1 implementation

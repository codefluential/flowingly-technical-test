# M3 Execution Plan - UI & E2E Tests

**Generated**: 2025-10-06
**Milestone**: M3 (tasks 041-050)
**Objective**: Demonstrate working flow with E2E validation
**Duration**: 4 hours
**Submittable After**: task_050

---

## Executive Summary

### Task File Status
✅ **All 10 M3 task files created** (task_041.json through task_050.json)

### Optimization Opportunities Identified

1. **NEW Parallelization Opportunity**:
   - **task_041** (Enhance UI) + **task_044** (Setup Playwright) can run **in parallel**
   - Both depend only on task_040 (M2 DoD gate)
   - No shared dependencies
   - **Recommendation**: Add `M3_parallel_0` group

2. **Existing Parallelization** (already defined):
   - **M3_parallel_1**: task_045, task_046, task_047 (E2E tests) run concurrently after task_044

3. **Agent Assignment Issues**:
   - ✅ **task_042**: Changed from `spec-mobile-react-native` → `frontend-design-expert` (React/TypeScript, not mobile)
   - ✅ **task_044**: Changed from `production-validator` → `quality-assurance-engineer` (test infrastructure setup)

---

## Task Dependency Graph

```
task_040 (M2 DoD Gate) ✅
    ├─→ task_041 (Enhance UI) [1h] **CAN RUN IN PARALLEL**
    │       ↓
    │   task_042 (TypeScript Types) [30min]
    │       ↓
    │   task_043 (Error Display) [45min]
    │
    └─→ task_044 (Setup Playwright) [30min] **CAN RUN IN PARALLEL**
            ↓
        [M3_parallel_1: RUN CONCURRENTLY]
        ├─→ task_045 (E2E Happy Path) [1h]
        ├─→ task_046 (E2E Error Tests) [1h]
        └─→ task_047 (E2E GST Verification) [45min]
                ↓
            task_048 (Run Full Test Suite) [30min]
                ↓
            task_049 (Manual Smoke Test) [20min]
                ↓
            task_050 (M3 & Phase 1 DoD - SUBMITTABLE) [20min] ✅
```

---

## Execution Sequence Options

### Option 1: Sequential (Conservative)
**Total Time**: ~6.5 hours

```
1. task_041 (1h)
2. task_042 (30min)
3. task_043 (45min)
4. task_044 (30min)
5. task_045 (1h) ──┐
6. task_046 (1h)   │ Sequential execution
7. task_047 (45min)┘
8. task_048 (30min)
9. task_049 (20min)
10. task_050 (20min)
```

### Option 2: Existing Parallel Groups (Current)
**Total Time**: ~5 hours

```
1. task_041 (1h)
2. task_042 (30min)
3. task_043 (45min)
4. task_044 (30min)
5. [PARALLEL] task_045 + task_046 + task_047 (1h longest)
6. task_048 (30min)
7. task_049 (20min)
8. task_050 (20min)
```

### Option 3: Optimized Parallel (RECOMMENDED)
**Total Time**: ~4 hours (matches milestone target)

```
1. [PARALLEL] task_041 (1h) + task_044 (30min) = 1h
2. task_042 (30min)
3. task_043 (45min)
4. [PARALLEL] task_045 + task_046 + task_047 (1h longest)
5. task_048 (30min)
6. task_049 (20min)
7. task_050 (20min)

Timeline: 1h + 30min + 45min + 1h + 30min + 20min + 20min = ~4h
```

---

## Recommended Parallelization Changes

### Add New Parallel Group: `M3_parallel_0`

**Update tasks.json**:
```json
"M3_parallel_0": {
  "description": "UI enhancement and test setup can run concurrently",
  "tasks": ["task_041", "task_044"],
  "after": "task_040"
}
```

**Rationale**:
- Both depend only on task_040
- task_041: Frontend UI work (React components)
- task_044: Backend test infrastructure (Playwright setup)
- Zero shared file dependencies
- Saves 30 minutes of sequential time

### Keep Existing: `M3_parallel_1`

```json
"M3_parallel_1": {
  "description": "All E2E tests can run concurrently",
  "tasks": ["task_045", "task_046", "task_047"],
  "after": "task_044"
}
```

---

## Agent Assignment Review

### ✅ Appropriate Assignments

| Task | Agent | Rationale |
|------|-------|-----------|
| task_041 | frontend-design-expert | UI enhancement, React components, accessibility |
| task_042 | frontend-design-expert | TypeScript types (was spec-mobile-react-native ❌) |
| task_043 | frontend-design-expert | Error display UI component |
| task_044 | quality-assurance-engineer | Playwright setup (was production-validator ❌) |
| task_045 | production-validator | E2E happy path testing |
| task_046 | production-validator | E2E error path testing |
| task_047 | production-validator | E2E GST verification |
| task_048 | production-validator | Full test suite execution |
| task_049 | production-validator | Manual smoke testing |
| task_050 | production-validator | Final DoD verification & submission |

### ⚠️ Agent Assignment Corrections Made

1. **task_042**: `spec-mobile-react-native` → `frontend-design-expert`
   - **Why**: React web app TypeScript types, not React Native mobile
   - **Impact**: Task file created with correct agent

2. **task_044**: `production-validator` → `quality-assurance-engineer`
   - **Why**: Test infrastructure setup, not validation
   - **Impact**: Task file created with correct agent

**Action Required**: Update `tasks.json` to reflect these corrections.

---

## Task Breakdown by Type

### Frontend UI Tasks (3 tasks, 2h15m)
- task_041: Enhance UI Components (1h)
- task_042: Add TypeScript Types (30min)
- task_043: Implement Error Display (45min)

**Agent**: frontend-design-expert
**Can Start**: After task_040 completes
**Deliverables**: Enhanced React components, TypeScript types, error UI

### Test Infrastructure (1 task, 30m)
- task_044: Setup Playwright (30min)

**Agent**: quality-assurance-engineer
**Can Start**: After task_040 completes (PARALLEL with task_041)
**Deliverables**: Playwright config, test structure, CI integration

### E2E Testing (3 tasks, 1h max parallel)
- task_045: Write E2E Happy Path Tests (1h)
- task_046: Write E2E Error Tests (1h)
- task_047: Write E2E GST Verification (45min)

**Agent**: production-validator
**Can Start**: After task_044 completes
**Parallel Group**: M3_parallel_1
**Deliverables**: 5+ E2E tests (happy path, error cases, GST verification)

### Quality Gates (3 tasks, 1h10m)
- task_048: Run Full Test Suite (30min)
- task_049: Manual Smoke Test (20min)
- task_050: Verify M3 & Phase 1 DoD - SUBMITTABLE (20min)

**Agent**: production-validator
**Sequential**: Must run after E2E tests complete
**Deliverables**: Test reports, verification checklists, submission readiness

---

## Critical Path Analysis

### Longest Path (Without Optimization)
```
task_040 → task_041 (1h) → task_042 (30m) → task_043 (45m) → task_044 (30m)
→ task_045 (1h) → task_048 (30m) → task_049 (20m) → task_050 (20m)
= 5h15m
```

### Optimized Path (With M3_parallel_0)
```
task_040 → [task_041 || task_044] (1h) → task_042 (30m) → task_043 (45m)
→ [task_045 || task_046 || task_047] (1h) → task_048 (30m) → task_049 (20m) → task_050 (20m)
= 4h25m (within 4h milestone target ✅)
```

---

## Resource Allocation

### Frontend Resources
- **frontend-design-expert**: 3 tasks (task_041, task_042, task_043)
- **Duration**: 2h15m sequential
- **Availability**: Can start immediately after task_040

### Testing Resources
- **quality-assurance-engineer**: 1 task (task_044)
- **Duration**: 30min
- **Availability**: Can start immediately after task_040 (parallel with task_041)

- **production-validator**: 6 tasks (task_045-050)
- **Duration**: 3h10m (1h parallel, 2h10m sequential)
- **Availability**: After task_044 for E2E tests

### Parallel Execution Windows

**Window 1** (after task_040):
- task_041 (frontend-design-expert) + task_044 (quality-assurance-engineer)
- No resource conflicts

**Window 2** (after task_044):
- task_045 + task_046 + task_047 (all production-validator)
- **Note**: Assumes production-validator can handle 3 parallel tasks OR use 3 separate validator instances

---

## Risk Assessment

### High Risk Items

1. **E2E Test Environment**
   - **Risk**: API + UI servers must be running for E2E tests
   - **Mitigation**: task_048 includes server startup verification
   - **Impact**: Could block tasks 045-047 if environment not ready

2. **Playwright Installation**
   - **Risk**: Browser binaries are large (~1GB), may fail on CI
   - **Mitigation**: task_044 includes installation verification
   - **Impact**: Could block entire E2E testing phase

3. **GST Calculation Accuracy**
   - **Risk**: Banker's Rounding is critical (120.50 → 104.78 + 15.72)
   - **Mitigation**: task_047 explicitly tests this case
   - **Impact**: Incorrect rounding will fail test brief submission

### Medium Risk Items

1. **TypeScript Type Mismatches**
   - **Risk**: Frontend types may not match backend DTOs exactly
   - **Mitigation**: task_042 includes field-by-field comparison
   - **Impact**: Could cause runtime errors in UI

2. **Accessibility Compliance**
   - **Risk**: WCAG AA compliance not verified automatically
   - **Mitigation**: task_041 includes manual accessibility checklist
   - **Impact**: Professional impression affected

### Low Risk Items

1. **UI Styling**
   - **Risk**: Responsive design may have edge cases
   - **Mitigation**: task_041 includes responsive breakpoints
   - **Impact**: Minor UX issues

---

## Success Criteria

### M3 Definition of Done (from task_050)

1. ✅ All 10 M3 tasks completed (task_041-050)
2. ✅ Enhanced UI functional (task_041, task_042, task_043)
3. ✅ Playwright setup complete (task_044)
4. ✅ 5+ E2E tests passing (task_045, task_046, task_047)
5. ✅ Full test suite passing: 45+ total (30 unit, 10 contract, 5 E2E)
6. ✅ Manual smoke test complete (task_049)
7. ✅ Zero test failures
8. ✅ BUILDLOG.md updated with M3 completion
9. ✅ Application SUBMITTABLE to Flowingly

### Phase 1 Submission Readiness (from task_050)

1. ✅ All 50 tasks complete (M0, M1, M2, M3)
2. ✅ 45+ tests passing (100% pass rate)
3. ✅ All parsing rules working (tag validation, GST, Banker's Rounding)
4. ✅ API functional end-to-end
5. ✅ UI functional end-to-end
6. ✅ Documentation complete (README, ADRs, BUILDLOG)
7. ✅ Code quality verified (Clean Architecture, 0 warnings)
8. ✅ Repository clean (no uncommitted changes)
9. ✅ 2-minute reviewer quick start works
10. ✅ Submission artifacts ready

---

## Recommended Execution Workflow

### Phase 1: UI Enhancement (2h15m)
```bash
# Terminal 1: Mark task_041 in progress
./scripts/update-progress.sh task_041 in_progress

# Parallel Terminal 2: Mark task_044 in progress
./scripts/update-progress.sh task_044 in_progress

# Execute task_041 and task_044 in parallel
# (frontend-design-expert + quality-assurance-engineer)

# After task_041 completes (1h):
./scripts/update-progress.sh task_041 completed

# After task_044 completes (30min):
./scripts/update-progress.sh task_044 completed

# Sequential: task_042, task_043
./scripts/update-progress.sh task_042 in_progress
# Execute task_042 (30min)
./scripts/update-progress.sh task_042 completed

./scripts/update-progress.sh task_043 in_progress
# Execute task_043 (45min)
./scripts/update-progress.sh task_043 completed
```

### Phase 2: E2E Testing (1h parallel)
```bash
# Start all 3 E2E tests in parallel
./scripts/update-progress.sh task_045 in_progress
./scripts/update-progress.sh task_046 in_progress
./scripts/update-progress.sh task_047 in_progress

# Execute in parallel (production-validator)
# All complete after ~1h (longest test duration)

./scripts/update-progress.sh task_045 completed e2e 2
./scripts/update-progress.sh task_046 completed e2e 2
./scripts/update-progress.sh task_047 completed e2e 1
```

### Phase 3: Quality Gates (1h10m)
```bash
# Sequential execution
./scripts/update-progress.sh task_048 in_progress
# Execute task_048 - verify 45+ tests passing
./scripts/update-progress.sh task_048 completed

./scripts/update-progress.sh task_049 in_progress
# Execute task_049 - manual smoke test
./scripts/update-progress.sh task_049 completed

./scripts/update-progress.sh task_050 in_progress
# Execute task_050 - FINAL DoD VERIFICATION
./scripts/update-progress.sh task_050 completed
```

### Total Time: ~4h25m (within 4h milestone target)

---

## Verification Commands

### After Each Task
```bash
# Verify task status
jq '.tasks[] | select(.id=="task_041") | {id, status, started_at, completed_at}' \
  project-context/implementation/tasks/tasks.json

# Verify progress
cat project-context/implementation/PROGRESS.md | grep "M3"
```

### After M3 Completion
```bash
# Verify all 50 tasks complete
jq '.progress_tracking.tasks_completed' project-context/implementation/tasks/tasks.json
# Expected: 50

# Verify test counts
jq '.progress_tracking.tests_passing' project-context/implementation/tasks/tasks.json
# Expected: {unit: 30+, contract: 10+, e2e: 5+, total: 45+}

# Verify M3 milestone complete
jq '.progress_tracking.milestones_completed' project-context/implementation/tasks/tasks.json
# Expected: ["M0", "M1", "M2", "M3"]
```

---

## Next Steps

1. **Update tasks.json** with agent corrections:
   - task_042: agent → "frontend-design-expert"
   - task_044: agent → "quality-assurance-engineer"

2. **Add M3_parallel_0** to parallel_execution_groups (optional optimization)

3. **Execute M3 tasks** following optimized workflow above

4. **Monitor progress** via PROGRESS.md dashboard

5. **Final submission** after task_050 completes

---

## Summary

✅ **All 10 M3 task files created**
✅ **2 agent assignment issues identified and corrected**
✅ **1 new parallelization opportunity identified** (saves 30 minutes)
✅ **Optimized execution plan reduces time from 5h15m to 4h25m**
✅ **Critical path analysis complete**
✅ **Risk assessment documented**
✅ **Verification workflow provided**

**Recommendation**: Use **Option 3 (Optimized Parallel)** for fastest completion within milestone target.

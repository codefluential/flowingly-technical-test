# Phase 1 Task Decomposition

**Purpose**: Comprehensive task breakdown for Phase 1 (M0→M3) core submission with full context and references for autonomous execution.

---

## 📁 Files in This Directory

### Master Orchestration
- **`tasks.json`** - Master task file with:
  - All 50 Phase 1 tasks
  - Dependency graph
  - Parallel execution groups (5 groups)
  - Milestone gates (M0→M1→M2→M3)
  - Agent assignments
  - Global context references

### Individual Task Files
- **`task_001.json` through `task_050.json`** - Each fully self-contained with:
  - Task definition and description
  - Agent assignment with role
  - Duration estimate and priority
  - Dependencies and parallel grouping
  - **Complete context** (PRD, ADRs, test brief, delivery plan)
  - Deliverables and acceptance criteria
  - Business rules and technical notes
  - Code examples and validation steps
  - Next task linkage

---

## 📊 Task Statistics

### By Milestone
| Milestone | Tasks | Duration | Objective |
|-----------|-------|----------|-----------|
| **M0** | 001-010 (10 tasks) | 4 hours | Minimal scaffold |
| **M1** | 011-030 (20 tasks) | 1 day | Core parsing & validation |
| **M2** | 031-040 (10 tasks) | 4 hours | API contracts |
| **M3** | 041-050 (10 tasks) | 4 hours | UI & E2E tests |
| **Total** | **50 tasks** | **2.5 days** | ✅ Submittable product |

### By Agent Type
- **base-template-generator**: 1 task (scaffold)
- **backend-architect**: 2 tasks (architecture)
- **frontend-design-expert**: 4 tasks (UI)
- **project-organizer**: 3 tasks (docs/scripts)
- **tester**: 2 tasks (strategy)
- **tdd-london-swarm**: 10 tasks (TDD tests)
- **coder**: 8 tasks (implementation)
- **code-analyzer**: 1 task (quality)
- **dev-backend-api**: 6 tasks (API)
- **docs-api-openapi**: 2 tasks (Swagger)
- **quality-assurance-engineer**: 2 tasks (contract tests)
- **reviewer**: 1 task (review)
- **spec-mobile-react-native**: 1 task (TypeScript)
- **production-validator**: 7 tasks (E2E + verification)

---

## 🔗 Context & References

### Each Task Includes References To:

1. **PRD Technical Spec** (`prd-technical_spec.md`)
   - Specific section numbers
   - Exact requirements
   - BDD scenarios (Section 13)

2. **Test Brief PDF** (Full Stack Engineer Test)
   - Page numbers
   - Specific requirements
   - Grading criteria

3. **ADRs** (Architectural Decision Records)
   - Relevant decisions
   - Rationale and context
   - Key excerpts

4. **Delivery Plan** (`delivery-plan-optimized.md`)
   - Milestone objectives
   - Specific deliverables
   - DoD criteria

---

## 🚀 Parallel Execution Groups

### Safe Concurrent Execution
Tasks in the same parallel group can run simultaneously without conflicts:

**M0_parallel_1** (after task_002):
- task_003 (API structure)
- task_004 (UI structure)

**M1_parallel_1** (after task_013):
- task_014 (Tag validation tests)
- task_017 (Number normalization tests)
- task_019 (Banker's rounding tests)

**M1_parallel_2** (after task_020):
- task_021 (Tax calculator tests)
- task_023 (Time parser tests)
- task_025 (XML extractor tests)

**M2_parallel_1** (after task_036):
- task_037 (Contract tests)
- task_038 (Swagger examples)

**M3_parallel_1** (after task_044):
- task_045 (E2E happy path)
- task_046 (E2E error tests)
- task_047 (E2E GST verification)

**Total parallelizable tasks**: 15+ tasks can run concurrently

---

## 📋 TDD Workflow

### Red-Green-Refactor Cycle
Tasks follow strict TDD discipline:

1. **RED Phase** (tdd-london-swarm):
   - Write failing tests FIRST
   - Verify tests fail (RED is success!)

2. **GREEN Phase** (coder):
   - Implement to make tests pass
   - Minimal code to achieve GREEN

3. **REFACTOR Phase** (code-analyzer):
   - Quality analysis
   - Optimization suggestions

### TDD Task Pairs
- task_014 (RED) → task_015 (GREEN) → task_016 (REFACTOR) - Tag Validation
- task_017 (RED) → task_018 (GREEN) - Number Normalization
- task_019 (RED) → task_020 (GREEN) - Banker's Rounding
- task_021 (RED) → task_022 (GREEN) - Tax Calculator
- task_023 (RED) → task_024 (GREEN) - Time Parser
- task_025 (RED) → task_026 (GREEN) - XML Extractor
- task_027 (RED) → task_028 (GREEN) - Content Router
- task_029 (RED) → task_030 (GREEN) - Expense Processor

---

## ✅ Milestone Gates (DoD Tasks)

**Critical checkpoints that must pass before proceeding:**

- **task_010**: M0 DoD - Scaffold complete
- **task_030**: M1 DoD - All parsing tests green
- **task_040**: M2 DoD - API contract verified
- **task_050**: M3 & Phase 1 DoD - **✅ SUBMITTABLE PRODUCT**

---

## 📚 Sample Task Structure

### Fully Self-Contained Context
```json
{
  "task_id": "task_019",
  "name": "Write Banker's Rounding Tests",
  "context": {
    "prd_reference": {
      "file": "prd-technical_spec.md",
      "sections": ["Section 9.3: Banker's Rounding"]
    },
    "test_brief_reference": {
      "page": 3,
      "requirement": "GST calculation accuracy"
    },
    "adr_references": [
      {
        "file": "ADR-0009-bankers-rounding.md",
        "decision": "MidpointRounding.ToEven",
        "rationale": "Statistically unbiased"
      }
    ]
  },
  "test_scenarios": [
    {
      "input": 2.125,
      "expected": 2.12,
      "reason": "2 is even, rounds down"
    }
  ],
  "business_rules": [
    "MUST use MidpointRounding.ToEven",
    "Apply to ALL financial calculations"
  ],
  "code_examples": {
    "implementation": "Math.Round(value, 2, MidpointRounding.ToEven)"
  }
}
```

**Every task can be executed independently** with all necessary context embedded.

---

## 🎯 Key Principles

### AADF Compliance
- **Logical progression**: Foundation → Implementation → Quality → Documentation
- **Clear deliverables**: Each task has specific outputs
- **Traceable**: Brief → PRD → Delivery Plan → Tasks
- **Quality gates**: DoD checkpoints between milestones

### TDD/BDD Focus
- **Tests first**: Never implement before tests fail
- **BDD scenarios**: From PRD Section 13
- **London School**: Mockist style for fast, isolated tests
- **Coverage**: 45+ tests (unit + contract + E2E)

### Agent Expertise
- **Right agent, right task**: Optimal agent assignment per task type
- **Self-contained**: Each task has full context for autonomous execution
- **Collaboration**: Clear handoff patterns between agents

---

## 🔍 How to Use This Task System

### For Automated Execution
1. Load `tasks.json` to get dependency graph
2. Execute tasks in topological order respecting dependencies
3. Run parallel groups concurrently where safe
4. Verify DoD gates before milestone transitions
5. Reference individual `task_nnn.json` for full context

### For Manual Execution
1. Start at task_001, follow sequence
2. Read full task JSON for complete context
3. Check dependencies are complete before starting
4. Verify acceptance criteria when done
5. Proceed to next task via `next_task.id`

### For Agent Invocation
```bash
# Example: Execute task_014
# Agent receives full task context including:
# - PRD Section 10.1 (Tag Validation Rules)
# - ADR-0008 (Stack-based validation decision)
# - Test scenarios (7+ test cases)
# - Code examples (test structure)
# - Business rules (overlapping tags must fail)

/task task_014.json
```

---

## 📈 Progress Tracking

### Milestone Completion
- M0 complete when: task_010 passes (all M0 acceptance criteria ✅)
- M1 complete when: task_030 passes (all 30+ tests green ✅)
- M2 complete when: task_040 passes (contract tests + Swagger ✅)
- M3 complete when: task_050 passes (E2E tests + smoke test ✅)

### Phase 1 Complete = Submittable Product
After task_050 verification:
- ✅ Reviewer can clone and run in <5 minutes
- ✅ All parsing rules working (tag validation, GST, Banker's Rounding)
- ✅ API contract correct (expense XOR other)
- ✅ UI functional with E2E tests
- ✅ 45+ tests green (unit + contract + E2E)
- ✅ README with quick start guide

---

## 📝 Task File Status

### Created (4 tasks with full context):
✅ **tasks.json** - Master orchestration (50 tasks)
✅ **task_001.json** - Create Solution Structure (M0)
✅ **task_014.json** - Write Tag Validation Tests (M1, TDD RED)
✅ **task_019.json** - Write Banker's Rounding Tests (M1, TDD RED)
✅ **task_031.json** - Create DTOs (M2)

### To Be Created (46 remaining):
⏳ task_002 through task_013, task_015 through task_018, task_020 through task_030, task_032 through task_050

**Pattern established** - remaining tasks follow same comprehensive structure with full context and references.

---

## 🚀 Next Steps

1. **Create remaining task files** using established pattern
2. **Validate task graph** for circular dependencies
3. **Test parallel execution** safety
4. **Begin implementation** at task_001

---

**Last Updated**: 2025-10-06
**Version**: 1.0 - Phase 1 Task Decomposition
**Total Tasks**: 50
**Estimated Completion**: 2.5 days
**Outcome**: ✅ Submittable product meeting all test brief requirements

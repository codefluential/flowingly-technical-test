# Task Creation Guide

Comprehensive guide for creating self-contained, context-rich task files for autonomous agent execution.

## Table of Contents
1. [Environment Setup](#environment-setup)
2. [Overview](#overview)
3. [Source Documents](#source-documents)
4. [Task File Structure](#task-file-structure)
5. [Field-by-Field Guide](#field-by-field-guide)
6. [Task Type Patterns](#task-type-patterns)
7. [Creation Process](#creation-process)
8. [Quality Checklist](#quality-checklist)

---

## Environment Setup

### .NET 8 Path Configuration
**CRITICAL FOR ALL BACKEND TASKS**: dotnet is installed at `/home/adarsh/.dotnet/dotnet` (version 8.0.414).

**ALL dotnet commands MUST include PATH export**:
```bash
export PATH="$HOME/.dotnet:$PATH" && dotnet <command>
```

**Examples for task files**:
```json
{
  "validation": {
    "command": "export PATH=\"$HOME/.dotnet:$PATH\" && dotnet test --filter FullyQualifiedName~TagValidator",
    "expected_output": "Test run successful"
  }
}
```

**Do NOT write**:
```json
// ❌ WRONG - will fail
"command": "dotnet test"

// ✅ CORRECT - will work
"command": "export PATH=\"$HOME/.dotnet:$PATH\" && dotnet test"
```

---

## Overview

### Philosophy
**"Each task is a complete knowledge package"** - An agent should execute a task without hunting across documents.

### Goals
- ✅ **Self-contained**: All context embedded in task file
- ✅ **Traceable**: Direct references to source documents with sections/pages
- ✅ **Actionable**: Clear deliverables and acceptance criteria
- ✅ **Validated**: Specific commands to verify completion
- ✅ **Connected**: Clear dependencies and next steps

### Statistics (Current)
- **Created**: 4 sample tasks (001, 014, 019, 031)
- **Remaining**: 46 tasks to create
- **Pattern established**: M0 scaffold, M1 TDD, M2 API contracts

---

## Source Documents

All task context is extracted from these canonical sources:

### 1. Primary Specification
**File**: `project-context/specifications/prd-technical_spec.md`
**Version**: v0.3 (current)
**Sections to Reference**:
- Section 4: Technology Stack
- Section 7: Architecture (Clean/Hexagonal + CQRS-lite)
- Section 10: Parsing Rules (Tag validation, XML extraction)
- Section 10.1: Tag Validation Rules
- Section 11: Request/Response Contracts
- Section 11.2: Response Contract - Classification-Specific (XOR)
- Section 12: Tax Calculation (Banker's Rounding, GST)
- Section 13: BDD Scenarios

**Usage**: Extract exact requirements, business rules, technical specs

### 2. Test Brief (Requirements)
**File**: `project-context/requirements-and-analysis/Full Stack Engineer Test (Sen) V2.pdf`
**Pages**: 1-3
**Content**:
- Page 1: Setup phase, project scope
- Page 2: Tag validation requirements, grading criteria
- Page 3: API contract, GST calculation, submission requirements

**Usage**: Reference specific page numbers, grading criteria, test scenarios

### 3. Architecture Decision Records (10 ADRs)
**Location**: `project-context/adr/`

| ADR | File | Decision | When to Reference |
|-----|------|----------|-------------------|
| 0001 | ADR-0001-storage-choice.md | SQLite dev, Postgres prod | Database tasks |
| 0002 | ADR-0002-architecture-style.md | Clean/Hexagonal + CQRS | All architecture tasks |
| 0003 | ADR-0003-processor-strategy.md | Strategy pattern for processors | Content routing tasks |
| 0004 | ADR-0004-swagger-api-contract.md | Swagger for API docs | API endpoint, DTO tasks |
| 0005 | ADR-0005-versioning-via-uri.md | URI versioning `/api/v1/` | All API tasks |
| 0006 | ADR-0006-api-key-auth.md | API key authentication | Auth tasks (M4+) |
| 0007 | ADR-0007-response-contract-design.md | Expense XOR Other response | DTO, response tasks |
| 0008 | ADR-0008-parsing-validation-rules.md | Stack-based tag validation | Tag validator tasks |
| 0009 | ADR-0009-tax-calculation-rounding.md | Banker's Rounding (MidpointRounding.ToEven) | Tax calculator tasks |
| 0010 | ADR-0010-test-strategy-coverage.md | 45+ tests (30 unit, 10 contract, 5 E2E) | All test tasks |

**Usage**:
- Include decision summary
- Extract key rationale
- Provide relevant code examples
- Reference specific sections

### 4. Delivery Plan
**File**: `project-context/planning/delivery-plan-optimized.md`
**Sections**:
- M0 — Minimal Scaffold (10 tasks, 4h)
- M1 — Core Parsing & Validation (20 tasks, 1 day)
- M2 — API Contracts (10 tasks, 4h)
- M3 — UI & E2E Tests (10 tasks, 4h)

**Usage**: Reference milestone section, DoD criteria, time estimates

### 5. Master Task Orchestration
**File**: `project-context/implementation/tasks/tasks.json`
**Contains**:
- All 50 task definitions (id, name, agent, duration, dependencies)
- Parallel execution groups (5 groups)
- Milestone assignments
- Progress tracking structure

**Usage**: Extract task metadata, dependencies, agent assignments

### 6. Build Log (Historical Context)
**File**: `project-context/build-logs/BUILDLOG.md`
**Usage**: Reference implementation history, patterns established, lessons learned

---

## Task File Structure

### JSON Schema
```json
{
  "task_id": "task_XXX",
  "milestone": "MX",
  "name": "Clear Task Name",
  "description": "Detailed description with context and purpose",

  "agent": {
    "primary": "agent-name",
    "role": "What this agent does for this task"
  },

  "duration": "Xh or XXmin",
  "priority": "critical|high|medium|low",
  "dependencies": ["task_XXX", "task_YYY"],
  "parallel_group": "MX_parallel_Y" or null,
  "tdd_phase": "RED|GREEN|REFACTOR" (M1 tasks only),

  "context": {
    "prd_reference": {
      "file": "project-context/specifications/prd-technical_spec.md",
      "sections": [
        "Section X.Y: Title - Key requirement",
        "Section Z: Title - Related context"
      ],
      "requirements": [
        "Extracted requirement 1",
        "Extracted requirement 2"
      ]
    },
    "test_brief_reference": {
      "file": "project-context/requirements-and-analysis/Full Stack Engineer Test (Sen) V2.pdf",
      "page": X,
      "requirement": "Specific requirement from brief",
      "grading_criteria": "What this affects for grading"
    },
    "adr_references": [
      {
        "file": "project-context/adr/ADR-XXXX-name.md",
        "decision": "Summary of decision",
        "rationale": "Why this decision was made",
        "context": "Background and problem",
        "key_excerpt": "Critical quote or rule from ADR"
      }
    ],
    "delivery_plan_reference": {
      "file": "project-context/planning/delivery-plan-optimized.md",
      "section": "MX — Milestone Name",
      "sub_section": "Specific component",
      "requirement": "What delivery plan requires"
    }
  },

  "deliverables": [
    {
      "item": "FileName.ext",
      "description": "What this file does",
      "location": "relative/path/to/file",
      "properties": { /* optional: key properties */ }
    }
  ],

  "acceptance_criteria": [
    "✅ Criterion 1 - specific and measurable",
    "✅ Criterion 2 - verifiable",
    "✅ Criterion 3 - testable"
  ],

  "business_rules": [
    "Rule 1 from PRD or ADR",
    "Rule 2 that must be followed",
    "Rule 3 extracted from specs"
  ],

  "technical_notes": {
    "commands": "dotnet/npm commands to use",
    "patterns": "Design patterns to apply",
    "dependencies": "NuGet/npm packages needed"
  },

  "code_examples": {
    "interface": "Code snippet showing interface",
    "usage": "Code snippet showing usage",
    "test_structure": "Code snippet showing test pattern"
  },

  "validation": {
    "command": "Command to verify completion",
    "expected_output": "What success looks like"
  },

  "next_task": {
    "id": "task_YYY",
    "name": "Next Task Name",
    "why": "Why this task follows logically"
  }
}
```

---

## Field-by-Field Guide

### Core Identity Fields

#### `task_id` (required)
- Format: `task_XXX` (3-digit zero-padded)
- Example: `"task_001"`, `"task_014"`, `"task_031"`

#### `milestone` (required)
- Values: `"M0"`, `"M1"`, `"M2"`, `"M3"`
- Corresponds to delivery plan milestones

#### `name` (required)
- Clear, action-oriented name
- Examples:
  - M0: `"Create Solution Structure"`, `"Setup API Endpoint Structure"`
  - M1: `"Write Tag Validation Tests (TDD)"`, `"Implement ITagValidator"`
  - M2: `"Create DTOs"`, `"Write API Contract Tests"`

#### `description` (required)
- 1-2 sentences explaining purpose and context
- Include "why" and "what" (not just "what")
- For TDD tasks, specify RED/GREEN/REFACTOR phase

### Agent Assignment

#### `agent.primary` (required)
- Must match an agent in `.claude/agents/` or be createable
- Common agents:
  - `base-template-generator` (M0 scaffolding)
  - `backend-architect` (API, architecture)
  - `frontend-design-expert` (React, UI)
  - `tdd-london-swarm` (TDD tests)
  - `dev-backend-api` (API implementation)

#### `agent.role` (required)
- Brief description of agent's role for this specific task
- Example: `"TDD Test Implementation (London School - mockist style)"`

### Scheduling Fields

#### `duration` (required)
- Format: `"Xh"` or `"XXmin"`
- Examples: `"30min"`, `"1h"`, `"2h"`
- Be realistic based on task complexity

#### `priority` (required)
- Values: `"critical"`, `"high"`, `"medium"`, `"low"`
- Most tasks are `"critical"` or `"high"` for M0-M3

#### `dependencies` (required, can be empty array)
- Array of task IDs that must complete first
- Example: `["task_001", "task_002"]`
- Empty for first tasks: `[]`

#### `parallel_group` (required, can be null)
- Groups tasks that can run concurrently
- Format: `"MX_parallel_Y"` or `null`
- Examples:
  - `"M0_parallel_1"` (task_003, task_004)
  - `"M1_parallel_1"` (multiple validator tests)
  - `null` (sequential task)

#### `tdd_phase` (optional, M1 only)
- Values: `"RED"`, `"GREEN"`, `"REFACTOR"`
- Only include for M1 TDD tasks

### Context Section (MOST IMPORTANT)

#### `context.prd_reference` (required)
**Purpose**: Embed PRD requirements directly in task

```json
"prd_reference": {
  "file": "project-context/specifications/prd-technical_spec.md",
  "sections": [
    "Section 10.1: Tag Validation Rules - Stack-based validator required",
    "Section 13: BDD Scenarios - 'Scenario: Overlapping Tags Should Be Rejected'"
  ],
  "requirements": [
    "Reject overlapping tags like <a><b></a></b>",
    "Reject unclosed tags like <a><b>",
    "Accept proper nesting like <a><b></b></a>",
    "Use stack-based algorithm (not regex)"
  ]
}
```

**How to Create**:
1. Read relevant PRD sections
2. Extract section numbers and titles
3. List specific requirements as bullet points
4. Include BDD scenarios if applicable

#### `context.test_brief_reference` (required)
**Purpose**: Link to test requirements and grading criteria

```json
"test_brief_reference": {
  "file": "project-context/requirements-and-analysis/Full Stack Engineer Test (Sen) V2.pdf",
  "page": 2,
  "requirement": "Validate tag integrity and reject malformed content",
  "grading_criteria": "Tag validation is a core graded component"
}
```

**How to Create**:
1. Identify relevant page in test brief PDF
2. Extract specific requirement text
3. Note what grading criteria this affects

#### `context.adr_references` (required, can be empty array)
**Purpose**: Include architectural decisions and rationale

```json
"adr_references": [
  {
    "file": "project-context/adr/ADR-0008-parsing-validation-rules.md",
    "decision": "Stack-based Tag Validation (Not Regex Balance)",
    "rationale": "Detect overlapping tags, not just unclosed tags",
    "context": "Why overlapping tags like <a><b></a></b> must be rejected",
    "key_excerpt": "A stack-based parser can detect when closing tags don't match the most recent opening tag"
  }
]
```

**How to Create**:
1. Identify relevant ADRs for this task
2. Summarize the decision
3. Extract rationale (the "why")
4. Include key excerpt/quote if critical
5. For 0-1 ADRs: `[]` if none apply

#### `context.delivery_plan_reference` (required)
**Purpose**: Link to milestone and DoD criteria

```json
"delivery_plan_reference": {
  "file": "project-context/planning/delivery-plan-optimized.md",
  "section": "M1 — Core Parsing & Validation",
  "sub_section": "Tag Validation",
  "requirement": "Implement stack-based validator tracking open/close tags"
}
```

### Deliverables Section

#### `deliverables` (required)
**Purpose**: List all files/artifacts to create

```json
"deliverables": [
  {
    "item": "TagValidatorTests.cs",
    "description": "xUnit test class with FluentAssertions",
    "location": "api/tests/Flowingly.ParsingService.Tests/Validators/",
    "test_count": "7+ test methods",
    "properties": { /* optional */ }
  }
]
```

**Guidelines**:
- One entry per file or artifact
- Include full relative path in `location`
- Add domain-specific fields (like `test_count`, `properties`)

### Acceptance Criteria

#### `acceptance_criteria` (required)
**Purpose**: Define "done" - specific, measurable, verifiable

```json
"acceptance_criteria": [
  "✅ 7+ test methods created covering all scenarios",
  "✅ All tests use FluentAssertions syntax",
  "✅ All tests currently FAIL (no implementation yet)",
  "✅ Test names follow Given_When_Then or Should pattern",
  "✅ Each test has clear Arrange-Act-Assert structure"
]
```

**Guidelines**:
- Start each with `"✅ "`
- Be specific and measurable
- Include quantity targets (e.g., "7+ tests", "0 warnings")
- Make it verifiable (agent can check completion)

### Business Rules

#### `business_rules` (required)
**Purpose**: Extract non-negotiable rules from specs/ADRs

```json
"business_rules": [
  "Stack-based validation is MANDATORY (not regex)",
  "Overlapping tags MUST be rejected (not just unclosed)",
  "Proper nesting follows LIFO (Last In First Out) principle",
  "Error code must be UNCLOSED_TAGS for all tag errors",
  "Validation happens before any content processing"
]
```

**Guidelines**:
- Extract from PRD, ADRs, test brief
- Use uppercase for emphasis (MUST, MANDATORY, NEVER)
- Include default values (e.g., "Default tax rate: 0.15")
- List validation rules, error codes, calculations

### Technical Notes

#### `technical_notes` (optional but recommended)
**Purpose**: Provide technical guidance

```json
"technical_notes": {
  "dotnet_command": "dotnet new xunit -n Flowingly.ParsingService.Tests",
  "nuget_packages": ["FluentAssertions", "xunit"],
  "design_pattern": "Arrange-Act-Assert (AAA) pattern",
  "naming_convention": "Given_When_Then or Should_X_When_Y"
}
```

### Code Examples

#### `code_examples` (highly recommended for implementation tasks)
**Purpose**: Show expected structure/patterns

```json
"code_examples": {
  "test_structure": "// Arrange\nvar validator = new TagValidator();\nvar input = \"<a><b></a></b>\";\n\n// Act\nAction act = () => validator.Validate(input);\n\n// Assert\nact.Should().Throw<ValidationException>()\n   .WithMessage(\"*UNCLOSED_TAGS*\");",
  "interface": "public interface ITagValidator\n{\n    ValidationResult Validate(string content);\n}",
  "expected_exception": "public class ValidationException : Exception\n{\n    public string ErrorCode { get; set; }\n}"
}
```

**Guidelines**:
- Include interface definitions
- Show usage examples
- Provide test patterns (for TDD tasks)
- Use `\n` for newlines in JSON strings

### Special Fields for TDD Tasks

#### `test_scenarios` (M1 RED tasks only)
**Purpose**: List all test cases with inputs/outputs

```json
"test_scenarios": [
  {
    "name": "Overlapping_Tags_Should_Be_Rejected",
    "input": "<a><b></a></b>",
    "expected": "ValidationException with UNCLOSED_TAGS",
    "reason": "Closing tag </a> doesn't match most recent opening <b>"
  },
  {
    "name": "Proper_Nesting_Should_Be_Accepted",
    "input": "<a><b></b></a>",
    "expected": "Valid (no exception)",
    "reason": "LIFO ordering maintained"
  }
]
```

#### `tdd_workflow` (M1 tasks only)
**Purpose**: Clarify TDD phase and next step

```json
"tdd_workflow": {
  "current_phase": "RED - Write failing tests",
  "next_phase": "task_015 - GREEN - Implement to make tests pass",
  "principle": "Never write implementation before tests fail"
}
```

### Validation Section

#### `validation` (required)
**Purpose**: Command to verify task completion

```json
"validation": {
  "command": "dotnet test --filter FullyQualifiedName~TagValidator",
  "expected_output": "7+ tests FAILED (RED)",
  "failure_is_success": true  // Only for RED phase
}
```

**For non-TDD tasks**:
```json
"validation": {
  "command": "dotnet build Flowingly.ParsingService.sln",
  "expected_output": "Build succeeded. 0 Warning(s) 0 Error(s)"
}
```

### Next Task

#### `next_task` (required)
**Purpose**: Link to following task and explain connection

```json
"next_task": {
  "id": "task_015",
  "name": "Implement ITagValidator",
  "why": "Tests are failing (RED), now make them pass (GREEN)",
  "tdd_phase": "GREEN"  // Optional, for TDD flow
}
```

---

## Task Type Patterns

### Pattern 1: M0 Scaffold Tasks
**Example**: task_001 (Create Solution Structure)

**Key Characteristics**:
- Focus on setup, structure, configuration
- No business logic yet
- Deliverables are project files, folders, configs
- Validation is "build succeeds" or "file exists"

**Template Fields to Emphasize**:
- `technical_notes` with commands
- `deliverables` with directory structure
- `code_examples` with dotnet/npm commands

**Example Context**:
```json
"context": {
  "prd_reference": {
    "sections": ["Section 4: Technology Stack", "Section 7: Architecture"],
    "requirements": ["Use .NET 8", "Clean/Hexagonal layers"]
  },
  "adr_references": [
    {
      "file": "ADR-0002-architecture-style.md",
      "decision": "Clean/Hexagonal Architecture",
      "relevance": "Defines project layer structure"
    }
  ]
}
```

### Pattern 2: M1 TDD RED Tasks
**Example**: task_014 (Write Tag Validation Tests)

**Key Characteristics**:
- Tests MUST fail initially
- Comprehensive test scenarios
- No implementation code
- Clear Arrange-Act-Assert structure

**Template Fields to Emphasize**:
- `test_scenarios` (detailed list)
- `tdd_phase`: `"RED"`
- `tdd_workflow` section
- `validation` with `failure_is_success: true`
- `code_examples.test_structure`

**Example Context**:
```json
"context": {
  "prd_reference": {
    "sections": ["Section 13: BDD Scenarios"],
    "requirements": ["Reject overlapping tags", "Use stack-based algorithm"]
  },
  "adr_references": [
    {
      "file": "ADR-0008-parsing-validation-rules.md",
      "decision": "Stack-based Tag Validation",
      "key_excerpt": "Stack detects when closing tags don't match opening"
    },
    {
      "file": "ADR-0010-test-strategy-coverage.md",
      "decision": "TDD with London School approach"
    }
  ]
}
```

### Pattern 3: M1 TDD GREEN Tasks
**Example**: task_015 (Implement ITagValidator)

**Key Characteristics**:
- Make RED tests pass
- Minimal implementation (no gold-plating)
- All tests must pass after implementation
- Follow design patterns from ADRs

**Template Fields to Emphasize**:
- `tdd_phase`: `"GREEN"`
- Reference to previous RED task
- `code_examples.interface` and `code_examples.implementation`
- `validation` with "all tests PASS"

### Pattern 4: M1 TDD REFACTOR Tasks
**Example**: task_016 (Refactor Tag Validator)

**Key Characteristics**:
- Improve code quality without changing behavior
- All tests still pass
- Address code smells, duplication, complexity

**Template Fields to Emphasize**:
- `tdd_phase`: `"REFACTOR"`
- `business_rules` about maintaining test coverage
- `acceptance_criteria` including "no new tests needed"

### Pattern 5: M2 API Contract Tasks
**Example**: task_031 (Create DTOs)

**Key Characteristics**:
- Focus on API contracts, DTOs, validation
- Swagger/OpenAPI compliance
- Type safety (C# + TypeScript alignment)

**Template Fields to Emphasize**:
- `deliverables` with DTO class names and properties
- `adr_references` to ADR-0007 (Response Contract)
- `code_examples` with DTO structure and Swagger attributes
- Reference to TypeScript type alignment

**Example Context**:
```json
"context": {
  "prd_reference": {
    "sections": ["Section 11.2: Response Contract - XOR"],
    "requirements": ["Response MUST be expense XOR other", "Never both"]
  },
  "adr_references": [
    {
      "file": "ADR-0007-response-contract-design.md",
      "decision": "Classification-Specific Response (Expense XOR Other)",
      "example_expense": "{ full JSON example }",
      "example_other": "{ full JSON example }"
    }
  ]
}
```

### Pattern 6: M3 UI Tasks
**Example**: task_041 (Enhance UI Components)

**Key Characteristics**:
- React component implementation
- TypeScript type safety
- Accessibility (ARIA, keyboard nav)
- Integration with API client

**Template Fields to Emphasize**:
- `technical_notes` with React patterns, Vite config
- `code_examples` with component structure
- `acceptance_criteria` including accessibility checks
- Reference to API contract for type alignment

### Pattern 7: M3 E2E Test Tasks
**Example**: task_045 (Write E2E Happy Path Tests)

**Key Characteristics**:
- Playwright test scenarios
- User-centric workflows
- Test against real API + UI

**Template Fields to Emphasize**:
- `test_scenarios` with user workflows
- `technical_notes` with Playwright patterns
- `code_examples.test_structure` with Playwright syntax
- `validation` with "X E2E tests pass"

---

## Creation Process

### Step-by-Step Workflow

#### 1. Extract Task Metadata
**Source**: `tasks.json`

```bash
# Extract task entry
jq '.tasks[] | select(.id == "task_003")' tasks.json
```

**Capture**:
- `task_id`, `name`, `milestone`
- `agent`, `duration`, `priority`
- `dependencies`, `parallel_group`

#### 2. Read Relevant ADRs
**Based on task type**:

| Task Type | ADRs to Read |
|-----------|--------------|
| M0 Scaffold | ADR-0002, ADR-0005 |
| M1 Validators | ADR-0008, ADR-0010 |
| M1 Tax Calc | ADR-0009, ADR-0010 |
| M2 DTOs | ADR-0007, ADR-0004, ADR-0005 |
| M3 UI | ADR-0004, ADR-0007 |

**Extract**:
- Decision summary
- Rationale
- Key excerpts
- Code examples from ADR

#### 3. Read PRD Sections
**Based on task domain**:

| Domain | PRD Sections |
|--------|--------------|
| Tag Validation | Section 10.1, Section 13 |
| Tax Calculation | Section 12, Section 13 |
| API Contracts | Section 11, Section 11.2 |
| Architecture | Section 7 |
| Technology | Section 4 |

**Extract**:
- Section titles and numbers
- Specific requirements (bullet points)
- BDD scenarios if applicable

#### 4. Read Test Brief Pages
**Based on task domain**:

| Domain | Test Brief Pages |
|--------|------------------|
| Setup | Page 1 |
| Validation | Page 2 |
| API/GST | Page 3 |

**Extract**:
- Specific requirements
- Grading criteria
- Sample inputs/outputs

#### 5. Read Delivery Plan
**Source**: `delivery-plan-optimized.md`

**Extract** (for task's milestone):
- Milestone section
- Sub-section for component
- DoD criteria
- Time allocation

#### 6. Determine Task Type Pattern
**Choose pattern** based on:
- Milestone (M0/M1/M2/M3)
- Task nature (scaffold/test/implementation/UI)
- TDD phase (RED/GREEN/REFACTOR for M1)

**Use pattern template** from "Task Type Patterns" section above

#### 7. Write Context Section
**Order**:
1. `prd_reference` - sections + requirements
2. `test_brief_reference` - page + requirement
3. `adr_references` - decision + rationale + excerpts
4. `delivery_plan_reference` - milestone + section

**Quality Check**:
- ✅ All source documents referenced
- ✅ Specific sections/pages cited
- ✅ Key requirements extracted
- ✅ ADR decisions summarized

#### 8. Define Deliverables
**For each file/artifact**:
- Item name (filename)
- Description (purpose)
- Location (relative path)
- Optional: properties, test count, etc.

**Example**:
```json
{
  "item": "ParseEndpoint.cs",
  "description": "Minimal API endpoint for /api/v1/parse",
  "location": "src/Api/Endpoints/",
  "properties": {
    "http_method": "POST",
    "route": "/api/v1/parse",
    "returns": "ParseResponse"
  }
}
```

#### 9. Write Acceptance Criteria
**Guidelines**:
- Start with `"✅ "`
- Be specific and measurable
- Include quantities (e.g., "0 warnings", "5+ tests")
- Make it verifiable
- Cover functional + non-functional requirements

**Example**:
```json
"acceptance_criteria": [
  "✅ Endpoint responds to POST /api/v1/parse",
  "✅ Returns 200 OK with ParseResponse",
  "✅ Includes correlation ID in response",
  "✅ Swagger documentation generated",
  "✅ Solution builds with 0 warnings"
]
```

#### 10. Extract Business Rules
**Sources**:
- PRD requirements
- ADR decisions
- Test brief constraints

**Example**:
```json
"business_rules": [
  "URI versioning: /api/v1/ (from ADR-0005)",
  "Response includes correlation ID (from PRD Section 11)",
  "Default tax rate: 0.15 NZ GST (from PRD Section 12)",
  "Expense XOR Other response (from ADR-0007)"
]
```

#### 11. Add Code Examples
**For implementation tasks**:
- Interface definitions
- Class structure
- Usage examples

**For test tasks**:
- Test method structure
- Arrange-Act-Assert pattern
- FluentAssertions syntax

**Example**:
```json
"code_examples": {
  "endpoint": "app.MapPost(\"/api/v1/parse\", ([FromBody] ParseRequest req) => \n{\n  var correlationId = Guid.NewGuid().ToString();\n  return Results.Ok(new ParseResponse { CorrelationId = correlationId });\n});",
  "swagger_attribute": "[SwaggerOperation(\n  Summary = \"Parse text content\",\n  Description = \"Extracts structured data from text\"\n)]"
}
```

#### 12. Define Validation
**Command to verify completion**:

**Examples**:
```json
// Build task
"validation": {
  "command": "dotnet build",
  "expected_output": "Build succeeded. 0 Warning(s) 0 Error(s)"
}

// Test task (RED)
"validation": {
  "command": "dotnet test --filter FullyQualifiedName~TagValidator",
  "expected_output": "7+ tests FAILED (RED)",
  "failure_is_success": true
}

// Test task (GREEN)
"validation": {
  "command": "dotnet test --filter FullyQualifiedName~TagValidator",
  "expected_output": "7+ tests PASSED"
}
```

#### 13. Link Next Task
**From tasks.json dependency graph**:

```json
"next_task": {
  "id": "task_004",
  "name": "Bootstrap React+Vite Frontend",
  "why": "API structure ready, now create frontend to consume it"
}
```

#### 14. Add TDD-Specific Fields (M1 Only)
**For RED tasks**:
```json
"tdd_phase": "RED",
"test_scenarios": [ /* list of scenarios */ ],
"tdd_workflow": {
  "current_phase": "RED - Write failing tests",
  "next_phase": "task_015 - GREEN - Implement to make tests pass",
  "principle": "Never write implementation before tests fail"
}
```

**For GREEN tasks**:
```json
"tdd_phase": "GREEN",
"tdd_workflow": {
  "current_phase": "GREEN - Make tests pass",
  "next_phase": "task_016 - REFACTOR - Improve code quality",
  "principle": "Simplest implementation that makes tests pass"
}
```

---

## Quality Checklist

Before finalizing a task file, verify:

### Context Completeness
- [ ] PRD sections referenced with section numbers
- [ ] Test brief page cited with specific requirement
- [ ] Relevant ADRs included with decision summary
- [ ] Delivery plan milestone and section referenced
- [ ] All requirements extracted (not just referenced)

### Traceability
- [ ] Can trace back to source documents
- [ ] Source document paths are correct
- [ ] Section/page numbers are accurate
- [ ] ADR file names match actual files

### Self-Containment
- [ ] Agent doesn't need to read other documents
- [ ] All business rules extracted
- [ ] Code examples provided
- [ ] Clear acceptance criteria

### Actionability
- [ ] Deliverables clearly defined
- [ ] File locations specified
- [ ] Validation command provided
- [ ] Expected outcomes clear

### Correctness
- [ ] Task ID matches tasks.json
- [ ] Dependencies are correct
- [ ] Agent assignment matches tasks.json
- [ ] Duration is realistic
- [ ] Next task is correct

### Pattern Adherence
- [ ] Follows correct pattern for task type
- [ ] TDD tasks have tdd_phase field
- [ ] TDD RED tasks have test_scenarios
- [ ] M2 tasks reference ADR-0007 for DTOs
- [ ] M0 tasks reference ADR-0002 for architecture

### JSON Validity
- [ ] Valid JSON syntax
- [ ] All strings properly escaped
- [ ] All required fields present
- [ ] Newlines in code examples use `\n`

---

## Example: Creating task_003.json

Let's walk through creating `task_003.json` step-by-step:

### Step 1: Extract from tasks.json
```bash
jq '.tasks[] | select(.id == "task_003")' tasks.json
```

**Output**:
```json
{
  "id": "task_003",
  "name": "Setup API Endpoint Structure",
  "agent": "backend-architect",
  "milestone": "M0",
  "duration": "45min",
  "dependencies": ["task_002"],
  "parallel_group": "M0_parallel_1"
}
```

### Step 2: Identify Relevant ADRs
- ADR-0002: Architecture (Clean/Hexagonal)
- ADR-0004: Swagger for API contract
- ADR-0005: URI versioning `/api/v1/`
- ADR-0007: Response contract (Expense XOR Other)

### Step 3: Identify PRD Sections
- Section 7: Architecture
- Section 11: Request/Response Contracts
- Section 11.2: Response Contract - XOR

### Step 4: Identify Test Brief Pages
- Page 1: Setup phase
- Page 3: API contract requirements

### Step 5: Choose Pattern
**Pattern**: M0 Scaffold Task

### Step 6-14: Create task file
*(See actual task_003.json creation below)*

---

## Maintenance

### When to Update This Guide
- New task patterns emerge
- New source documents added
- Task structure changes
- Quality issues identified

### Version History
- v1.0 (2025-10-06): Initial creation based on 4 sample tasks

---

**Last Updated**: 2025-10-06
**Status**: Comprehensive guide ready for task creation
**Next**: Create remaining 46 task files using this guide

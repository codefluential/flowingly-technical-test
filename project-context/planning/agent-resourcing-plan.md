# Agent Resourcing Plan — Flowingly Parsing Service

**Purpose**: Map optimal agent teams to each milestone for maximum delivery effectiveness.
**Strategy**: Phase 1 focuses on speed/correctness, Phase 2 on polish, Phase 3 on production readiness.
**Reference**: [Optimized Delivery Plan](./delivery-plan-optimized.md) | [Agent Library](../agents/all-agents-library/README.md)

---

## Quick Reference Table

| Milestone | Timeline | Agent Team | Primary Focus |
|-----------|----------|------------|---------------|
| **M0** | 4 hours | base-template-generator, backend-architect, frontend-design-expert, project-organizer | Minimal scaffold |
| **M1** | 1 day | coder, tester, tdd-london-swarm, code-analyzer | Core parsing & validation |
| **M2** | 4 hours | dev-backend-api, docs-api-openapi, reviewer, quality-assurance-engineer | API contracts |
| **M3** | 4 hours | frontend-design-expert, spec-mobile-react-native, production-validator | UI & E2E tests |
| **M6** | 4 hours | docs-maintainer, docs-api-openapi, reviewer | Documentation |
| **M4** | 4 hours | postgres-function-architect, supabase-schema-architect, postgres-style-enforcer | Persistence (optional) |
| **M5** | 4 hours | devops-deployment-architect, ops-cicd-github, production-validator | Production deployment |

---

## Phase 1: Core Submission (M0→M3) — 2.5 Days

### M0: Minimal Scaffold (4 hours)

**Objective**: Prove end-to-end wiring with minimal setup friction

#### Recommended Agent Team

1. **base-template-generator** (Lead)
   - **Role**: Create project structure for .NET 8 API and React+Vite frontend
   - **Deliverables**: Solution scaffolding, project references, basic configuration
   - **Why**: Specialized in rapid project setup with best practices

2. **backend-architect** (Architecture)
   - **Role**: Design Clean/Hexagonal architecture structure
   - **Deliverables**: Project layers (Api, Application, Domain, Infrastructure), DI setup
   - **Why**: Expert in .NET architecture patterns and CQRS-lite approach

3. **frontend-design-expert** (UI Setup)
   - **Role**: Bootstrap React+Vite with TypeScript configuration
   - **Deliverables**: Component structure, API client setup, basic styling
   - **Why**: Ensures proper frontend architecture from the start

4. **project-organizer** (Coordination)
   - **Role**: Set up README, scripts, development workflow
   - **Deliverables**: Quick-start documentation, Makefile/scripts, folder organization
   - **Why**: Creates frictionless developer experience

#### Collaboration Pattern
```
base-template-generator → Creates initial structure
    ↓
backend-architect + frontend-design-expert → Configure architecture (parallel)
    ↓
project-organizer → Document setup and workflows
```

#### Agent Invocation Example
```
"Use base-template-generator to scaffold a .NET 8 Web API with Clean Architecture and React+Vite frontend"
"Have backend-architect design the project layers following Clean/Hexagonal pattern with CQRS-lite"
"Deploy frontend-design-expert to set up React with TypeScript and minimal component structure"
"Engage project-organizer to create README with 3-step quick start"
```

---

### M1: Core Parsing & Validation (1 day)

**Objective**: Lock ALL parsing rules that the test brief will evaluate

#### Recommended Agent Team

1. **coder** (Lead Implementation)
   - **Role**: Implement parsers, validators, tax calculators as pure functions
   - **Deliverables**: ITagValidator, IXmlIslandExtractor, ITaxCalculator, INumberNormalizer
   - **Why**: Core development expertise for algorithm implementation

2. **tester** (Test Strategy)
   - **Role**: Design comprehensive test strategy for all parsing rules
   - **Deliverables**: Test plan covering happy paths, edge cases, error scenarios
   - **Why**: Ensures complete test coverage from the start

3. **tdd-london-swarm** (TDD Implementation)
   - **Role**: Implement test-first approach with mockist style
   - **Deliverables**: 30+ unit tests with FluentAssertions, test fixtures
   - **Why**: London School TDD ensures proper isolation and fast tests

4. **code-analyzer** (Quality Check)
   - **Role**: Analyze parser implementation for edge cases and performance
   - **Deliverables**: Code metrics, complexity analysis, optimization suggestions
   - **Why**: Catches issues early in critical parsing logic

#### Collaboration Pattern
```
tester → Defines test scenarios from spec
    ↓
tdd-london-swarm → Writes failing tests first
    ↓
coder → Implements parsers to pass tests
    ↓
code-analyzer → Reviews implementation quality
```

#### Agent Invocation Example
```
"Deploy tester to create test plan for tag validation, GST calculation, and number normalization"
"Use tdd-london-swarm to write unit tests for all parsing rules using xUnit and FluentAssertions"
"Have coder implement the parser domain logic as pure functions"
"Run code-analyzer to verify parser implementation quality and edge case handling"
```

---

### M2: API Contract & Error Handling (4 hours)

**Objective**: Wrap parser in HTTP API with correct contracts

#### Recommended Agent Team

1. **dev-backend-api** (Lead)
   - **Role**: Implement .NET 8 Minimal API endpoints with proper contracts
   - **Deliverables**: POST /api/v1/parse endpoint, error handling, correlation IDs
   - **Why**: Specialized in API development with .NET

2. **docs-api-openapi** (Contract Documentation)
   - **Role**: Create OpenAPI specification and Swagger setup
   - **Deliverables**: swagger.json, Swagger UI configuration, request/response examples
   - **Why**: Ensures API contracts are well-documented

3. **reviewer** (Contract Validation)
   - **Role**: Review API contracts against specification
   - **Deliverables**: Contract compliance report, consistency checks
   - **Why**: Catches contract mismatches before testing

4. **quality-assurance-engineer** (Integration Testing)
   - **Role**: Create WebApplicationFactory contract tests
   - **Deliverables**: 10+ API contract tests, error scenario validation
   - **Why**: Ensures API behaves correctly under all conditions

#### Collaboration Pattern
```
dev-backend-api → Implements API endpoints
    ↓
docs-api-openapi → Documents contracts (parallel with implementation)
    ↓
reviewer → Validates contracts match spec
    ↓
quality-assurance-engineer → Tests all scenarios
```

#### Agent Invocation Example
```
"Use dev-backend-api to implement POST /api/v1/parse with FluentValidation"
"Have docs-api-openapi create OpenAPI spec with discriminated union for expense/other responses"
"Deploy reviewer to validate API contracts match PRD specifications"
"Engage quality-assurance-engineer to write WebApplicationFactory tests for all scenarios"
```

---

### M3: Minimal UI & E2E Tests (4 hours)

**Objective**: Demonstrate working end-to-end flow with functional UI

#### Recommended Agent Team

1. **frontend-design-expert** (Lead)
   - **Role**: Implement React components for parser UI
   - **Deliverables**: ParserApp component, API client, response display
   - **Why**: Frontend architecture expertise

2. **spec-mobile-react-native** (React Implementation)
   - **Role**: Build React components with TypeScript
   - **Deliverables**: Form components, error handling, loading states
   - **Why**: React development expertise (applicable to web React too)

3. **production-validator** (E2E Testing)
   - **Role**: Create Playwright E2E test suite
   - **Deliverables**: 5+ E2E tests covering happy paths and errors
   - **Why**: Ensures end-to-end functionality works correctly

#### Collaboration Pattern
```
frontend-design-expert → Designs component structure
    ↓
spec-mobile-react-native → Implements React components
    ↓
production-validator → Creates E2E test suite
```

#### Agent Invocation Example
```
"Use frontend-design-expert to design minimal React UI with textarea and parse button"
"Have spec-mobile-react-native implement the React components with TypeScript"
"Deploy production-validator to create Playwright E2E tests using test brief samples"
```

---

## Phase 2: Polish & Enhancement (M6, M4) — 1 Day

### M6: Documentation & Developer Experience (4 hours)

**Objective**: Professional polish for documentation and ease of review

#### Recommended Agent Team

1. **docs-maintainer** (Lead)
   - **Role**: Create comprehensive README and developer guides
   - **Deliverables**: Quick start guide, architecture overview, troubleshooting
   - **Why**: Documentation expertise for developer experience

2. **docs-api-openapi** (API Documentation)
   - **Role**: Enhance Swagger with examples and descriptions
   - **Deliverables**: Complete OpenAPI spec, request/response examples
   - **Why**: API documentation specialist

3. **reviewer** (Documentation Review)
   - **Role**: Validate documentation completeness and accuracy
   - **Deliverables**: Documentation quality report, missing sections identification
   - **Why**: Ensures documentation meets professional standards

#### Collaboration Pattern
```
docs-maintainer → Creates comprehensive documentation
    ↓
docs-api-openapi → Enhances API documentation
    ↓
reviewer → Reviews for completeness
```

---

### M4: Persistence Layer (4 hours, Optional)

**Objective**: Add audit trail for requests/responses

#### Recommended Agent Team

1. **postgres-function-architect** (Lead)
   - **Role**: Design database schema for SQLite/Postgres
   - **Deliverables**: Schema design, migration scripts, indexes
   - **Why**: Database architecture expertise

2. **supabase-schema-architect** (Schema Design)
   - **Role**: Create optimized schema for message storage
   - **Deliverables**: Tables for messages, expenses, other_payloads
   - **Why**: Modern database schema patterns

3. **postgres-style-enforcer** (Standards)
   - **Role**: Ensure database code follows best practices
   - **Deliverables**: Naming conventions, query optimization
   - **Why**: Maintains database code quality

#### Collaboration Pattern
```
postgres-function-architect → Designs schema
    ↓
supabase-schema-architect → Optimizes for use case
    ↓
postgres-style-enforcer → Enforces standards
```

---

## Phase 3: Production Ready (M5) — 4 Hours

### M5: Observability & Deployment

**Objective**: Deploy to Render with production features

#### Recommended Agent Team

1. **devops-deployment-architect** (Lead)
   - **Role**: Design deployment strategy for Render
   - **Deliverables**: render.yaml, environment configuration, health checks
   - **Why**: Deployment architecture expertise

2. **ops-cicd-github** (CI/CD)
   - **Role**: Set up GitHub Actions for automated deployment
   - **Deliverables**: CI/CD pipeline, test automation, deployment workflow
   - **Why**: GitHub Actions specialist

3. **production-validator** (Validation)
   - **Role**: Validate production readiness
   - **Deliverables**: Production checklist, security review, performance validation
   - **Why**: Ensures production quality standards

#### Collaboration Pattern
```
devops-deployment-architect → Designs deployment
    ↓
ops-cicd-github → Implements CI/CD
    ↓
production-validator → Validates readiness
```

---

## Cross-Milestone Agents

These agents are used across multiple milestones:

| Agent | Used In | Purpose |
|-------|---------|---------|
| **reviewer** | M2, M6 | Code and documentation review |
| **production-validator** | M3, M5 | E2E testing and production validation |
| **docs-api-openapi** | M2, M6 | API documentation throughout |
| **quality-assurance-engineer** | M2, M3 | Testing at multiple levels |

---

## Alternative Agent Options

If primary agents are unavailable, consider these alternatives:

| Primary Agent | Alternative | Trade-offs |
|---------------|-------------|------------|
| **base-template-generator** | **coder** + **project-organizer** | More manual setup |
| **tdd-london-swarm** | **tester** alone | Less TDD rigor |
| **frontend-design-expert** | **coder** with React experience | Less specialized |
| **postgres-function-architect** | **supabase-schema-architect** alone | Less PostgreSQL-specific |
| **devops-deployment-architect** | **coder** with DevOps experience | Less specialized |

---

## Agent Invocation Templates

### For Planning Phase
```
"I need the planner agent to break down the M1 parsing implementation into subtasks"
"Use project-organizer to structure the codebase according to Clean Architecture"
```

### For Implementation Phase
```
"Deploy the coder agent to implement the ITagValidator interface with stack-based validation"
"Have dev-backend-api create the POST /api/v1/parse endpoint with proper error handling"
```

### For Testing Phase
```
"Use tdd-london-swarm to write unit tests for the GST calculator with Banker's Rounding"
"Deploy production-validator to create Playwright E2E tests for the happy path"
```

### For Documentation Phase
```
"Have docs-maintainer create a comprehensive README with quick-start instructions"
"Use docs-api-openapi to generate Swagger documentation with request/response examples"
```

### For Deployment Phase
```
"Deploy devops-deployment-architect to set up Render deployment configuration"
"Use ops-cicd-github to create GitHub Actions workflow for automated deployment"
```

---

## Success Metrics

### Phase 1 Agent Teams (M0-M3)
- **Team Size**: 13 unique agents across 4 milestones
- **Specialization Coverage**: Architecture, Implementation, Testing, Documentation
- **Delivery Time**: 2.5 days to submittable product

### Phase 2 Agent Teams (M6, M4)
- **Team Size**: 6 unique agents across 2 milestones
- **Focus**: Polish and optional features
- **Delivery Time**: 1 day for enhancement

### Phase 3 Agent Teams (M5)
- **Team Size**: 3 specialized agents
- **Focus**: Production deployment
- **Delivery Time**: 4 hours to production

---

## Key Recommendations

1. **Start with base-template-generator** in M0 for rapid scaffolding
2. **Use tdd-london-swarm** in M1 for test-first parser development
3. **Deploy specialized agents** (postgres-function-architect, devops-deployment-architect) only when needed
4. **Maintain reviewer presence** at critical checkpoints (M2, M6)
5. **Use production-validator** for both E2E testing (M3) and deployment validation (M5)

---

## Notes

- Each agent team is sized for optimal collaboration (3-5 agents per milestone)
- Agents are selected based on specialization match to milestone requirements
- Collaboration patterns ensure proper handoffs between agents
- Alternative agents provide flexibility if primary agents are unavailable

**Last Updated**: 2025-10-06
**Optimization**: Minimal viable teams for maximum delivery speed
# Project Context Documentation

This directory contains comprehensive documentation for the **Flowingly Parsing Service** project, including specifications, architecture decisions, planning artifacts, and development logs.

## =Ú Documentation Structure

### =Ë Specifications (`/specifications`)
**Primary specification (always use this):**
- **[prd-technical_spec.md](specifications/prd-technical_spec.md)**  PRD + Technical Specification **v0.3** (current)
  - Complete product requirements and technical design
  - API contracts, validation rules, error handling
  - Tax calculations (Banker's Rounding), parsing rules, normalization
  - Architecture patterns (Clean/Hexagonal + CQRS-lite)

**Archive:**
- Previous versions and review feedback (historical reference)

### <¯ Planning (`/planning`)
**Active plans:**
- **[delivery-plan.md](planning/delivery-plan.md)**  Milestone-based delivery roadmap (M0’M6)
  - Aligned with PRD v0.3
  - Test plan, acceptance checklist, implementation priorities
  - Engineering decisions and DoD for each milestone

**Archive:**
- `delivery-plan-doc-updates.md`  v0.2’v0.3 gap analysis (applied, archived)

### <Û Architecture Decision Records (`/adr`)
**Active ADRs (10 total):**
- **[ADR-0001-v2](adr/ADR-0001-v2-storage-choice.md)**  Storage: SQLite local/test, Postgres deployment
- **[ADR-0002](adr/ADR-0002-architecture-style.md)**  Clean/Hexagonal Architecture + CQRS-lite
- **[ADR-0003](adr/ADR-0003-processor-strategy-pattern.md)**  Processor Strategy (Expense vs Other)
- **[ADR-0004](adr/ADR-0004-swagger-api-contract.md)**  Swagger for API contract documentation
- **[ADR-0005](adr/ADR-0005-versioning-via-uri.md)**  URI-based API versioning (`/api/v1/`)
- **[ADR-0006](adr/ADR-0006-api-key-authentication.md)**  API key authentication
- **[ADR-0007](adr/ADR-0007-response-contract-design.md)**  Response contract: expense XOR other
- **[ADR-0008](adr/ADR-0008-parsing-validation-rules.md)**  Parsing & validation rules (stack-based tags)
- **[ADR-0009](adr/ADR-0009-bankers-rounding.md)**  Banker's Rounding (MidpointRounding.ToEven)
- **[ADR-0010](adr/ADR-0010-test-strategy-coverage.md)**  Test strategy & coverage

See **[adr/README.md](adr/README.md)** for complete index and status.

**Archive:**
- Superseded ADRs and update plans

### =Ê Requirements & Analysis (`/requirements-and-analysis`)
- **Test brief PDF**  Original technical test requirements
- **[requirements-analysis.md](requirements-and-analysis/requirements-analysis.md)**  Analysis of requirements

### =( Build Logs (`/build-logs`)
- **[BUILDLOG.md](build-logs/BUILDLOG.md)**  Chronological development history
  - Date, changes, rationale, issues, testing notes
  - Always update after significant work

### =€ Implementation (`/implementation`)
- Implementation guides and task tracking
- Currently in planning phase (no code yet)

### > Agents (`/agents`)
- AI agent definitions and templates
- Not actively used for this project (general library)

### =æ Archive (`/archive`)
- Historical documents and early development notes

### <“ Learnings (`/learnings`)
- Technical guides and MCP server documentation

---

## <¯ Quick Navigation for Common Tasks

### Starting Implementation
1. **Read:** [prd-technical_spec.md](specifications/prd-technical_spec.md) (v0.3)
2. **Review:** [delivery-plan.md](planning/delivery-plan.md) (milestones M0’M6)
3. **Check:** [adr/README.md](adr/README.md) (architectural decisions)
4. **Update:** [build-logs/BUILDLOG.md](build-logs/BUILDLOG.md) (after changes)

### Understanding Architecture
- **[ADR-0002](adr/ADR-0002-architecture-style.md)**  Architecture pattern
- **[ADR-0003](adr/ADR-0003-processor-strategy-pattern.md)**  Content routing strategy
- **[ADR-0007](adr/ADR-0007-response-contract-design.md)**  API response design

### Validation & Testing Rules
- **[ADR-0008](adr/ADR-0008-parsing-validation-rules.md)**  Parsing & validation
- **[ADR-0009](adr/ADR-0009-bankers-rounding.md)**  Rounding policy
- **[ADR-0010](adr/ADR-0010-test-strategy-coverage.md)**  Test coverage

### Storage & Deployment
- **[ADR-0001-v2](adr/ADR-0001-v2-storage-choice.md)**  Database strategy
- **[delivery-plan.md](planning/delivery-plan.md)**  M4 (SQLite), M5 (Postgres migration)

---

## =Ð Key Engineering Decisions (Lock Early)

| Decision | Value | Reference |
|----------|-------|-----------|
| **Rounding Policy** | Banker's Rounding (MidpointRounding.ToEven) | ADR-0009 |
| **Tag Validation** | Stack-based nesting (detect overlaps) | ADR-0008 |
| **Response Contract** | Classification-specific (expense XOR other) | ADR-0007 |
| **Database** | SQLite local/test, Postgres prod (M5+) | ADR-0001-v2 |
| **Architecture** | Clean/Hexagonal + CQRS-lite | ADR-0002 |
| **API Versioning** | URI-based (`/api/v1/`) | ADR-0005 |
| **Tax Rate Precedence** | Request > Config > Error/Default (0.15) | PRD v0.3 |

---

## = Documentation Maintenance

### When to Update
- **After major decisions:** Create/update ADRs
- **After implementation work:** Update BUILDLOG.md
- **When requirements change:** Update PRD (version increment)
- **When milestones shift:** Update delivery-plan.md

### Archive Policy
- Move superseded documents to `/archive` subdirectories
- Preserve for historical reference
- Update primary docs to reference archive when relevant

---

## =Ö Document Priorities

**Critical (always reference):**
1. [prd-technical_spec.md](specifications/prd-technical_spec.md)  Single source of truth
2. [delivery-plan.md](planning/delivery-plan.md)  Implementation roadmap
3. [adr/README.md](adr/README.md)  Architectural decisions index
4. [BUILDLOG.md](build-logs/BUILDLOG.md)  Development history

**Supporting:**
- ADR documents (specific decisions)
- Requirements analysis (original interpretation)
- Archive (historical context)

---

## =¦ Project Status

**Phase:** Planning complete, **ready for implementation**
**PRD Version:** v0.3 (current)
**Delivery Plan:** M0’M6 milestones defined
**ADRs:** 10 active decisions documented
**Code Implementation:** Not started (no .NET solution or React app yet)

See [BUILDLOG.md](build-logs/BUILDLOG.md) for detailed progress history.

---

## =¡ Tips for Using This Documentation

1. **Always start with the PRD**  It's the single source of truth
2. **Check ADRs before implementing**  Understand the "why" behind decisions
3. **Follow the delivery plan milestones**  Prioritize M0’M2 (core) before M3’M6
4. **Update BUILDLOG after work**  Maintain development history
5. **Archive superseded docs**  Keep active docs clean and current
6. **Reference ADRs in code**  Link decisions to implementation

---

**Last Updated:** 2025-10-06
**Current Version:** PRD v0.3, Delivery Plan v1.1 (aligned with PRD v0.3)

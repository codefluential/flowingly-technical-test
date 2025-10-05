# ADR Update Plan - PRD v0.3 Alignment

**Date**: 2025-10-06
**Status**: Active
**Purpose**: Track necessary updates to ADRs following PRD v0.3 specification changes

---

## Executive Summary

The PRD has been updated to **v0.3** (2025-10-06) with major changes based on external review feedback. This plan identifies required updates to existing ADRs and new ADRs to be created to maintain alignment between specification and architectural decisions.

**Total Impact**: 6 existing ADRs, 4 new ADRs required

---

## PRD v0.3 Key Changes Requiring ADR Updates

### 1. Storage Strategy Revision (Section 8, 16, 17, 14)
**Change**: SQLite for local dev/tests, Postgres for deployment stages (M5+)
- **Current ADR**: ADR-0001 states "Postgres for all environments"
- **Action**: **SUPERSEDE** ADR-0001 with new ADR-0001-v2

### 2. API Response Structure Change (Section 4.1, 6)
**Change**: Responses now specific (expense OR other, not combined)
- **Impact**: Architecture implications for response builders
- **Action**: **UPDATE** ADR-0003 (Processor Strategy) with response contract clarification
- **Action**: **NEW ADR** - ADR-0007: Response Contract Design

### 3. Parsing Rules Enhanced (Section 4.2)
**Change**: Added overlapping tag validation, Banker's Rounding policy, tax rate precedence, ambiguous time handling
- **Impact**: New validation/calculation strategies
- **Action**: **NEW ADR** - ADR-0008: Parsing and Validation Rules
- **Action**: **NEW ADR** - ADR-0009: Tax Calculation with Banker's Rounding

### 4. Testing Coverage Expanded (Section 13)
**Change**: Added 6 new BDD scenarios, testing matrix, sample email fixtures requirement
- **Impact**: Testing strategy and infrastructure
- **Action**: **NEW ADR** - ADR-0010: Test Strategy and Coverage

### 5. Documentation Improvements
**Change**: Swagger examples deferred (Section 7), M0→M2 priority guardrails (Section 18), two-minute reviewer path (Section 19), rate limiting backlog (Section 20.3)
- **Action**: **UPDATE** ADR-0004 (Swagger) with deferred enhancements section

---

## Detailed Update Plan

### Phase 1: Existing ADR Updates

#### 1.1. ADR-0001: Storage Choice
**Status**: Supersede with ADR-0001-v2
**Reason**: Fundamental strategy change from "Postgres everywhere" to "SQLite local, Postgres deployment"

**Required Changes**:
- **Context**: Update to reflect time constraints and iteration speed as primary drivers
- **Decision**: Change from "PostgreSQL for all environments" to "SQLite for local dev/tests, Postgres for deployment"
- **Consequences**:
  - Add SQLite benefits (fast, hermetic tests, no external dependencies)
  - Document migration path from SQLite → Postgres (Section 16 details)
  - Update implementation notes with SQLite-specific considerations
  - Add warning about Postgres-specific features (JSONB operations must be abstracted)
- **Alternatives**: Update "SQLite dev + Postgres prod" from "Rejected" to "Accepted"
- **Implementation**: Add SQLite setup instructions, migration validation steps

**New Sections**:
- "SQLite Considerations" (file-based, in-memory, feature limitations)
- "Postgres Migration Path" (EF Core migration validation, deployment stages M4/M5)
- "Schema Compatibility Testing" (how to ensure SQLite → Postgres parity)

**Approval**: Mark original ADR-0001 as **Superseded**, create ADR-0001-v2 with **Accepted** status

---

#### 1.2. ADR-0003: Processor Strategy Pattern
**Status**: Update
**Reason**: Response structure change impacts processor contract

**Required Changes**:
- **Consequences**: Add note about specific response structure (expense XOR other, never both)
- **Implementation Guidelines**: Update `ProcessingResult` example to show classification-specific responses
- **Contract Example**:
  ```csharp
  // Expense response
  {
    "classification": "expense",
    "expense": { /* expense fields */ },
    "meta": { /* metadata */ }
  }

  // Other response
  {
    "classification": "other",
    "other": { /* raw tags */ },
    "meta": { /* metadata */ }
  }
  ```

**New Sections**:
- "Response Contract Design" (briefly, defer details to ADR-0007)
- Reference to ADR-0007 for full response structure rationale

---

#### 1.3. ADR-0004: Swagger/OpenAPI
**Status**: Update
**Reason**: Swagger examples deferred to post M0→M2 (Section 7 clarification)

**Required Changes**:
- **Future Enhancements**: Move "Swagger Examples" section from implementation to "Future Enhancements (Post M0→M2)"
- **Implementation Guidelines**: Remove pressure to add examples during core delivery
- **Consequences**: Add note that basic Swagger UI is M0→M2 scope, enhanced examples are M3+ backlog

**New Sections**:
- "M0→M2 Scope Guardrails" (what's in, what's deferred)
- Reference PRD Section 18 priority guidelines

---

#### 1.4. README.md (ADR Index)
**Status**: Update
**Reason**: Update planned ADRs list to reflect new ADRs 0007-0010

**Required Changes**:
- Update "Planned ADRs" section to include:
  - ADR-0001-v2: Storage choice (SQLite local, Postgres deployment) [Supersedes ADR-0001]
  - ADR-0007: Response Contract Design
  - ADR-0008: Parsing and Validation Rules
  - ADR-0009: Tax Calculation with Banker's Rounding
  - ADR-0010: Test Strategy and Coverage

---

### Phase 2: New ADRs

#### 2.1. ADR-0007: Response Contract Design
**Priority**: High
**Rationale**: API contract is central to system design; v0.3 changes require architectural justification

**Sections**:
- **Context**:
  - API consumers (React UI, future clients) need clear response structure
  - Classification determines response content (expense vs other)
  - Alternative: combined response with both expense and other blocks (always include both, nullable)
  - Trade-off: specificity vs flexibility
- **Decision**: Responses are specific to classification (expense XOR other, never both)
- **Consequences**:
  - **Positive**: Clear contracts, smaller payloads, easier client-side type safety (TypeScript discriminated unions)
  - **Negative**: Clients must handle two response shapes (but this matches domain reality)
- **Alternatives Considered**:
  - Combined response (both blocks always present, one nullable): Rejected due to payload bloat and confusing nullability
  - Separate endpoints (`/parse/expense`, `/parse/other`): Rejected because classification is determined during parsing, not known upfront
- **Implementation**:
  - TypeScript discriminated union example
  - DTO design pattern (base response + specific derived types)

---

#### 2.2. ADR-0008: Parsing and Validation Rules
**Priority**: High
**Rationale**: PRD v0.3 Section 4.2 added critical rules (overlapping tags, tax rate precedence, ambiguous time handling)

**Sections**:
- **Context**:
  - Tag integrity validation must catch overlapping/improperly nested tags (e.g., `<a><b></a></b>`)
  - Tax rate precedence: request param > config default > error (if strict) or fallback 0.15
  - Ambiguous time handling: ignore unclear formats (log warning), only accept unambiguous formats
- **Decision**:
  - Stack-based parser with nesting validation (not just balance checking)
  - Tax rate precedence chain with configurable strict mode
  - Time parsing whitelist (explicit formats only)
- **Consequences**:
  - **Positive**: Robust validation, clear error messages, no silent failures
  - **Negative**: More complex parser logic, more test cases required
- **Alternatives Considered**:
  - Regex-only validation (rejected: can't detect nesting violations)
  - Always fallback to default tax rate (rejected: hides user errors)
  - Permissive time parsing (rejected: ambiguous inputs lead to incorrect data)
- **Implementation**:
  - Stack-based tag validator algorithm
  - Tax rate resolution chain
  - Time format whitelist

---

#### 2.3. ADR-0009: Tax Calculation with Banker's Rounding
**Priority**: Medium-High
**Rationale**: PRD v0.3 Section 4.2 specifies Banker's Rounding (MidpointRounding.ToEven) as policy

**Sections**:
- **Context**:
  - Monetary calculations require rounding at 2 decimal places
  - Standard rounding (0.5 always up) introduces bias over many operations
  - Tax calculations: total_incl_tax → total_excl_tax + sales_tax must be accurate
- **Decision**: Use Banker's Rounding (MidpointRounding.ToEven) for all monetary values
  - Examples: 2.125 → 2.12, 2.135 → 2.14 (rounds to nearest even)
- **Consequences**:
  - **Positive**: Unbiased rounding over many operations, statistical accuracy, matches financial standards
  - **Negative**: Less intuitive than standard rounding, requires explanation in docs
- **Alternatives Considered**:
  - Standard rounding (0.5 always up): Rejected due to upward bias
  - Truncation: Rejected due to downward bias
  - No rounding (exact decimal): Rejected as impractical for currency display
- **Implementation**:
  - `Math.Round(value, 2, MidpointRounding.ToEven)` in C#
  - Applied at business boundaries (after tax calculation, before persistence)
  - Unit tests for edge cases (2.125, 2.135, 2.145, 2.155)

---

#### 2.4. ADR-0010: Test Strategy and Coverage
**Priority**: Medium
**Rationale**: PRD v0.3 Section 13 significantly expanded test requirements (6 new BDD scenarios, testing matrix)

**Sections**:
- **Context**:
  - PRD v0.3 added overlapping tag tests, rounding policy tests, tax rate precedence tests
  - Testing matrix defines unit/integration/E2E coverage boundaries
  - Sample email fixtures from brief required for E2E tests
- **Decision**: Three-tier testing strategy aligned with PRD Section 13 matrix
  - **Unit Tests**: Domain logic (tag validator, tax calculator, normalizers) in isolation
  - **Integration Tests**: API endpoints with in-memory DB, full pipeline validation
  - **E2E Tests**: Playwright against running app + DB, sample email fixtures
- **Consequences**:
  - **Positive**: Comprehensive coverage, clear test ownership, prevents regressions
  - **Negative**: More test code to maintain, longer CI runs
- **Alternatives Considered**:
  - Unit tests only: Rejected (misses integration bugs)
  - E2E tests only: Rejected (slow, brittle, poor failure diagnostics)
  - Manual testing: Rejected (not repeatable, doesn't scale)
- **Implementation**:
  - BDD scenarios as xUnit test classes with Given/When/Then comments
  - Testing matrix as test plan document
  - Sample email fixtures in `/fixtures/` directory
  - Coverage targets: Unit 80%+, Integration 60%+, E2E happy + critical error paths

**New Testing Requirements from v0.3**:
- Overlapping tag validation (Scenario added)
- Banker's Rounding edge cases (Scenario added)
- Tax rate precedence (3 scenarios added)
- Sample Email 1 & 2 from brief (E2E fixtures required)

---

## Implementation Sequence

### Week 1: Critical Updates
1. **Day 1**: Supersede ADR-0001 with ADR-0001-v2 (storage strategy)
2. **Day 2**: Create ADR-0007 (response contract design)
3. **Day 3**: Create ADR-0008 (parsing/validation rules)

### Week 2: Enhancements
4. **Day 4**: Create ADR-0009 (Banker's Rounding)
5. **Day 5**: Create ADR-0010 (test strategy)
6. **Day 6**: Update ADR-0003 (processor strategy response notes)
7. **Day 7**: Update ADR-0004 (Swagger deferred enhancements) + README.md

---

## Validation Checklist

Before marking this plan complete, verify:

- [ ] All PRD v0.3 Document History changes (Section 6) mapped to ADR updates
- [ ] ADR-0001 properly superseded (old marked Superseded, new marked Accepted)
- [ ] ADR-0007 through ADR-0010 created with full sections
- [ ] ADR-0003 and ADR-0004 updated inline
- [ ] README.md ADR index updated with new entries
- [ ] All new ADRs cross-reference PRD v0.3 sections
- [ ] No orphaned decisions (all PRD v0.3 changes have corresponding ADR coverage)

---

## Approval and Sign-off

**Plan Created**: 2025-10-06
**Plan Approved**: [Pending]
**Implementation Start**: [Pending]
**Target Completion**: [7 days from approval]

**References**:
- PRD + Technical Specification v0.3 (Section 6: Document History)
- Existing ADRs: ADR-0001 through ADR-0006
- PRD Section 14: ADRs & Implementation Logs

---

## Appendix: PRD v0.3 Section Mapping

| PRD v0.3 Section | Change Summary | ADR Impact | Action |
|-----------------|----------------|------------|--------|
| Section 8 | Storage: SQLite local, Postgres deployment | ADR-0001 | Supersede with v2 |
| Section 4.1, 6 | API response: specific (expense XOR other) | ADR-0003, new ADR-0007 | Update + Create |
| Section 4.2 | Parsing rules: overlapping tags, Banker's Rounding, tax precedence, ambiguous time | New ADR-0008, ADR-0009 | Create |
| Section 13 | Testing: 6 new scenarios, matrix, fixtures | New ADR-0010 | Create |
| Section 7 | Swagger examples deferred to post-M2 | ADR-0004 | Update |
| Section 14, 16, 17 | Storage migration path documented | ADR-0001-v2 | Include in superseded ADR |
| Section 18 | M0→M2 priority guardrails | ADR-0004 | Update |
| Section 19 | Two-minute reviewer path | README.md | Update (not ADR) |
| Section 20.3 | Rate limiting backlog | Future work | Document in backlog (not ADR yet) |

**Total Sections Modified**: 12
**Total Lines Added/Updated in PRD**: ~250

---

## Next Steps

1. **Review this plan** with technical lead
2. **Prioritize ADRs** if time-constrained (focus on ADR-0001-v2, ADR-0007, ADR-0008)
3. **Create ADRs sequentially** following the implementation sequence
4. **Update BUILDLOG.md** with each ADR creation/update
5. **Cross-reference** new ADRs in PRD v0.3 (add footnotes if needed)

---

**End of ADR Update Plan**

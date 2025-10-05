# PRD v0.3 Update Plan — Based on External Review Feedback

**Date**: 2025-10-06
**Source**: prd-review-feedback-and-action.md
**Target Document**: prd-technical_spec.md (v0.2 → v0.3)
**Status**: Plan Ready for Implementation

---

## Executive Summary

This document provides a detailed, section-by-section plan for updating the PRD + Technical Specification from v0.2 to v0.3 based on external review feedback. The plan identifies 10 specific feedback points plus additional recommendations, maps each to PRD sections requiring changes, and specifies exact modifications.

**Major Changes:**
1. Storage strategy shift: SQLite for local/tests, Postgres for deployment (HIGH priority)
2. API response structure: Specific responses (expense OR other, not combined) (HIGH priority)
3. Testing enhancements: Overlapping tags, rounding policy, sample emails from brief (HIGH priority)
4. Validation clarifications: Tax rate precedence, ambiguous time handling (MEDIUM priority)
5. Documentation improvements: Two-minute reviewer path, backlog items (MEDIUM-LOW priority)

**After PRD Update**: Relevant ADRs will be updated (deferred per user instruction).

---

## Change Priority Matrix

| Priority | Feedback Point | Sections Affected | Impact |
|----------|----------------|-------------------|--------|
| **HIGH** | Storage strategy (SQLite/Postgres) | 8, 16, 17, 14 | Architectural |
| **HIGH** | API response structure (specific) | 4.1, 6 | API contract |
| **HIGH** | E2E test scenarios (sample emails) | 13 | Testing coverage |
| **HIGH** | M0→M2 prioritization | 18 | Scope/delivery |
| **MEDIUM-HIGH** | Overlapping tag validation | 4.2, 13 | Parsing rules |
| **MEDIUM** | Rounding policy | 4.2, 13 | Normalization |
| **MEDIUM** | Tax rate precedence | 4.2, 13 | Validation |
| **MEDIUM** | Two-minute reviewer path | 19 | Documentation |
| **LOW** | Ambiguous time handling | 4.2 | Edge case |
| **LOW** | Swagger examples (post-core) | 7 | Future enhancement |
| **LOW** | Rate limiting backlog | 20.3 | Future enhancement |

---

## Detailed Change Specifications

### **1. Storage Strategy Change (SQLite for Local/Tests, Postgres for Later Stages)**

**Priority**: HIGH
**Type**: Major architectural change
**Rationale**: Maximizes agility for M0→M2 delivery; provides fastest iteration locally

#### **Section 8: Data Model & Persistence** (lines 325-407)

**Current Text** (lines 327-330):
```markdown
**Storage Decision (updated)**

* **Single database: PostgreSQL on Render** for **Dev and Prod**. No SQLite.
* Benefits: one engine across environments, managed backups, easy connection rotation, and Render-native operations.
```

**Replace With**:
```markdown
**Storage Decision (updated v0.3)**

* **SQLite** for **local development** and **unit/integration tests** (fast, hermetic, no external dependencies)
* **PostgreSQL on Render** for **later deployment stages** (production-ready when ready to deploy)
* **Migration path**: EF Core migrations ensure schema compatibility; test SQLite schema against Postgres before deployment

**Rationale**: Given time constraints, SQLite provides fastest iteration locally while ensuring we can migrate to Postgres when hitting deployment milestones. This maximizes agility for M0→M2 delivery.

**SQLite Considerations**:
- Use file-based or in-memory SQLite for tests (hermetic, no setup required)
- Limit advanced Postgres-specific features (e.g., full JSONB operations) or abstract them behind repository interface
- Ensure EF Core migrations work with both providers (test on Postgres before production deployment)
```

**Current Text** (lines 332-334):
```markdown
**Render Postgres Notes**

* Managed PostgreSQL with versions **13–17** available for new instances; connection URLs exposed via dashboard. (See Render docs.)
```

**Replace With**:
```markdown
**Postgres Deployment Notes** (for later stages)

* **Render Managed PostgreSQL**: Versions **13–17** available for new instances; connection URLs exposed via dashboard
* **Migration from SQLite**:
  1. Run EF Core migrations against Postgres instance to create schema
  2. Verify schema matches SQLite version (no drift)
  3. Update connection string in production config
  4. Deploy to Render with Postgres connection
* **Testing**: Run integration tests against Postgres before production deployment to catch provider-specific issues
```

#### **Section 16: Deployment & DevOps** (lines 575-608)

**Current Text** (lines 590-593):
```markdown
**Deployment model**

* **Prod only** in Render, linked to the `main` branch.
* **Dev** runs **locally** (Docker Compose optional) for rapid iteration.
```

**Replace With**:
```markdown
**Deployment model**

* **Dev** runs **locally** with **SQLite** (file-based or in-memory) for rapid iteration and hermetic tests
  - No external database dependencies for local development
  - Tests run fast and isolated (no shared state between test runs)
* **Prod** in Render with **PostgreSQL** when ready for deployment (M5+ milestones)
  - Linked to `main` branch
  - Managed Postgres instance provisioned via Render dashboard
```

**Add After Line 608** (before "Environments" heading):
```markdown
**SQLite → Postgres Migration Path**

1. **Development**: Use SQLite for all local work and tests
2. **Pre-Deployment Validation** (M4/M5):
   - Provision Render Postgres instance
   - Run EF Core migrations: `dotnet ef database update --connection "PostgresConnectionString"`
   - Execute integration test suite against Postgres
   - Verify schema parity with SQLite version
3. **Deployment** (M5+):
   - Update production config with Postgres connection string
   - Deploy API to Render Web Service
   - Run smoke tests
```

#### **Section 17: Open Questions** (lines 609-621)

**Current Text** (line 618):
```markdown
6. **Storage:** **Postgres only**; Dev local, Prod Render.
```

**Replace With**:
```markdown
6. **Storage:** **SQLite for local dev/tests**; **Postgres for later deployment stages** (M5+). Migration path via EF Core ensures compatibility.
```

#### **Section 14: ADRs & Implementation Logs** (lines 551-562)

**Current Text** (line 555):
```markdown
  * ADR‑0001: **Storage choice → Single Postgres on Render (Dev & Prod)**
```

**Replace With**:
```markdown
  * ADR‑0001: **Storage choice → SQLite (local/test), Postgres (deployment)**
```

---

### **2. Add Overlapping Tag Validation**

**Priority**: MEDIUM-HIGH
**Type**: Parsing rule clarification
**Rationale**: Strengthens validation; catches malformed nested tags

#### **Section 4.2: Parsing & Validation Rules** (lines 178-193)

**Current Text** (line 180):
```markdown
* **Tag Integrity**: stack-based scan over `<name>` and `</name>`; must be balanced & properly nested.
```

**Replace With**:
```markdown
* **Tag Integrity**: stack-based scan over `<name>` and `</name>`; must be balanced & properly nested.
  - **Tag Overlap**: reject improperly nested/overlapping tags (e.g., `<a><b></a></b>` is invalid)
  - Stack-based parser ensures closing tags match most recent opening tag
  - Example valid: `<a><b></b></a>`
  - Example invalid: `<a><b></a></b>` → error `UNCLOSED_TAGS` or `MALFORMED_TAGS`
```

#### **Section 13: TDD/BDD Plan** (lines 483-548)

**Add to "Domain Unit" tests** (after line 488):
```markdown
  * Tag overlap validation (e.g., `<a><b></a></b>` → reject as `MALFORMED_TAGS`)
```

**Add BDD Scenario** (after line 526, before "Scenario: Missing cost_centre"):
```gherkin
  Scenario: Overlapping tags should error
    Given text with "<vendor><description></vendor></description>"
    When I POST it to /api/v1/parse
    Then the response status is 400
    And the error code is "UNCLOSED_TAGS" or "MALFORMED_TAGS"
    And the error details list the problematic tags
```

---

### **3. Add Rounding Policy**

**Priority**: MEDIUM
**Type**: Normalization rule clarification
**Rationale**: Ensures consistent, unbiased rounding behavior

#### **Section 4.2: Parsing & Validation Rules** (lines 178-193)

**Current Text** (line 190):
```markdown
  * Numbers: strip commas/currency symbols, parse as decimal, 2‑dp rounding at business boundaries.
```

**Replace With**:
```markdown
  * Numbers: strip commas/currency symbols, parse as decimal, **round using Banker's Rounding (MidpointRounding.ToEven)** at 2 decimal places for all monetary values.
    - **Banker's Rounding**: 0.5 rounds to nearest even number (unbiased over many operations)
    - Examples: 2.125 → 2.12, 2.135 → 2.14, 2.145 → 2.14, 2.155 → 2.16
    - Applied at business boundaries: tax calculation, total computations
```

#### **Section 13: TDD/BDD Plan** (lines 483-548)

**Add to "Domain Unit" tests** (after line 492):
```markdown
  * Rounding policy tests (verify Banker's Rounding: 2.125 → 2.12, 2.135 → 2.14, edge cases)
```

**Add BDD Scenario** (after line 546):
```gherkin
  Scenario: Rounding follows Banker's Rounding
    Given text with <expense><total>100.125</total></expense>
    When I POST it with taxRate 0.15
    Then expense.total_incl_tax is 100.12
    And sales_tax is computed with Banker's Rounding applied
    And total_excl_tax is computed with Banker's Rounding applied
```

---

### **4. Ignore Ambiguous Times**

**Priority**: LOW
**Type**: Edge case handling
**Rationale**: Prevents misinterpretation of unclear time formats

#### **Section 4.2: Parsing & Validation Rules** (lines 178-193)

**Current Text** (line 192):
```markdown
  * Time: if present, output `HH:mm`. **Dates vs UTC.** Business dates (like reservation/receipt dates) are **calendar dates**, not instants; we store them as date‑only. Audit fields (e.g., `received_at`) are stored in **UTC** (`TIMESTAMPTZ`) so server/DB clocks are consistent.
```

**Replace With**:
```markdown
  * Time: if present, attempt parsing to `HH:mm` format. **Ignore ambiguous times** (e.g., "7.30pm" with unclear separators, "19:30" without context) if not reliably detectable; log warning and omit time field in response.
    - Accept unambiguous formats: "19:30", "7:30 PM", "07:30"
    - Reject/ignore: "7.30pm" (dot separator ambiguous), partial times without clear AM/PM
  * **Dates vs UTC**: Business dates (like reservation/receipt dates) are **calendar dates**, not instants; we store them as date‑only. Audit fields (e.g., `received_at`) are stored in **UTC** (`TIMESTAMPTZ`) so server/DB clocks are consistent.
```

---

### **5. API Responses Should Be Specific (Either Expense OR Other, Not Combined)**

**Priority**: HIGH
**Type**: Major API contract change
**Rationale**: Cleaner API contract; simpler client-side handling

#### **Section 4.1: API** (lines 129-177)

**Current Text** (lines 144-169):
```json
  * Success `200`

    {
      "classification": "expense|other",
      "expense": {
        "cost_centre": "DEV632|UNKNOWN",
        "payment_method": "personal card",
        "total_incl_tax": 35000.00,
        "tax_rate": 0.15,
        "sales_tax": 4565.22,
        "total_excl_tax": 30434.78,
        "currency": "NZD",
        "source": "expense-xml|inline"
      },
      "other": {
        "raw_tags": ["vendor","description","date"],
        "note": "Stored as Other/Unprocessed for future processors"
      },
      "meta": {
        "warnings": [],
        "tags_found": ["expense","total","vendor","description","date"],
        "correlation_id": "guid"
      }
    }
```

**Replace With**:
```markdown
  * Success `200`

**Response Structure**: Responses are **specific to input context**. If classified as `expense`, response contains `expense` block only. If classified as `other`, response contains `other` block only. Never both in the same response.

**For Expense Content**:
    ```json
    {
      "classification": "expense",
      "expense": {
        "cost_centre": "DEV632|UNKNOWN",
        "payment_method": "personal card",
        "total_incl_tax": 35000.00,
        "tax_rate": 0.15,
        "sales_tax": 4565.22,
        "total_excl_tax": 30434.78,
        "currency": "NZD",
        "source": "expense-xml|inline"
      },
      "meta": {
        "warnings": [],
        "tags_found": ["expense","total","cost_centre"],
        "correlation_id": "guid"
      }
    }
    ```

**For Other/Unprocessed Content**:
    ```json
    {
      "classification": "other",
      "other": {
        "raw_tags": ["vendor","description","date"],
        "note": "Stored as Other/Unprocessed for future processors"
      },
      "meta": {
        "warnings": [],
        "tags_found": ["vendor","description","date"],
        "correlation_id": "guid"
      }
    }
    ```
```

#### **Section 6: Frontend (React)** (lines 211-243)

**Current Text** (line 220):
```markdown
* JSON viewer for result.
```

**Replace With**:
```markdown
* JSON viewer displays either expense details OR other/unprocessed details based on classification (responses are specific, not combined)
```

---

### **6. Rate Limiting as Backlog**

**Priority**: LOW
**Type**: Future enhancement documentation
**Rationale**: Acknowledges requirement but defers to post-v1

#### **Section 20.3: Future Enhancements** (lines 649-656)

**Add at line 655** (before closing the section):
```markdown
* **Rate Limiting**: Per-IP or per-API-key rate limiting using token bucket or sliding window middleware (backlog for post-v1). Prevents abuse and ensures fair usage. Implementation options: ASP.NET Core rate limiting middleware, custom middleware with distributed cache.
```

---

### **7. Tax Rate Precedence Clarification**

**Priority**: MEDIUM
**Type**: Validation rule clarification
**Rationale**: Removes ambiguity about fallback behavior

#### **Section 4.2: Parsing & Validation Rules** (lines 178-193)

**Add After Line 187** (after "Precedence" bullet):
```markdown
* **Tax Rate Precedence**: Request `taxRate` parameter wins over config default; if request omits `taxRate`, use config value; if both absent, **fail with error code `MISSING_TAXRATE`** or fallback to 0.15 (config-toggled behavior via `Parsing:StrictTaxRate`).
  - Example: Request has `taxRate: 0.10` → use 0.10 (ignores config)
  - Example: Request omits `taxRate`, config has `0.15` → use 0.15
  - Example: Request omits `taxRate`, config absent, `StrictTaxRate: true` → error 400 `MISSING_TAXRATE`
  - Example: Request omits `taxRate`, config absent, `StrictTaxRate: false` → fallback to 0.15 (default)
```

#### **Section 13: TDD/BDD Plan** (lines 483-548)

**Add to "Application/Integration" tests** (after line 498):
```markdown
  * POST with explicit taxRate → uses request value (ignores config)
  * POST without taxRate, config has default → uses config default
  * POST without taxRate, no config default, strict mode → 400 MISSING_TAXRATE
  * POST without taxRate, no config default, fallback mode → uses 0.15
```

**Add BDD Scenarios** (after line 546):
```gherkin
  Scenario: Request taxRate overrides config
    Given config has defaultTaxRate 0.15
    When I POST with taxRate 0.10
    Then expense.tax_rate is 0.10

  Scenario: Missing taxRate uses config default
    Given config has defaultTaxRate 0.15
    When I POST without taxRate parameter
    Then expense.tax_rate is 0.15

  Scenario: Missing taxRate with strict mode errors
    Given config has no defaultTaxRate and StrictTaxRate is true
    When I POST without taxRate parameter
    Then the response status is 400
    And the error code is "MISSING_TAXRATE"
```

---

### **8. Swagger Examples (Post-Core Delivery)**

**Priority**: LOW
**Type**: Future enhancement documentation
**Rationale**: Deferred to post-M2 to focus on core functionality

#### **Section 7: Backend (ASP.NET Core)** (lines 244-322)

**Add After Line 295** (after "Swagger (OpenAPI) — What & Why" section):
```markdown
**Swagger Examples (Future Enhancement — Post M0→M2)**

After core functionality (M0→M2) is delivered, enhance Swagger UI with:
- **Example Requests/Responses**: Use `[SwaggerRequestExample]` and `[SwaggerResponseExample]` attributes to show sample JSON for common scenarios
- **Reusable Config Cases**: Make examples accessible from demo app UI (user can choose scenario, execute via Swagger)
- **Scenarios**: Happy path expense, missing total error, overlapping tags error, tax rate precedence

This improves onboarding but is deferred to avoid scope creep during core delivery.
```

---

### **9. Add Both Sample Emails from Brief for E2E Tests**

**Priority**: HIGH
**Type**: Testing coverage enhancement
**Rationale**: Ensures parity with original requirements; comprehensive E2E validation

#### **Section 13: TDD/BDD Plan** (lines 483-548)

**Current Text** (lines 499-502):
```markdown
* **UI E2E (Playwright)**

  * Submit happy path → sees expense summary.
  * Submit invalid → sees error banner.
```

**Replace With**:
```markdown
* **UI E2E (Playwright)**

  * **Sample Email 1** (from brief — Expense with XML island): Submit → verify correct extraction, tax calculation, cost_centre handling
  * **Sample Email 2** (from brief — Expense with inline tags): Submit → verify correct extraction, field parsing
  * Submit invalid input (unclosed tags) → sees error banner with clear error code
  * Submit missing total → sees error banner with `MISSING_TOTAL`

**E2E Test Coverage**: Implement E2E tests using the **exact sample emails** from the original brief (`Full Stack Engineer Test (Sen) V2.pdf`, pages X-Y) to ensure complete scenario coverage and parity with requirements.
```

**Add After Line 547** (after last BDD scenario):
```markdown
---

**Test Fixtures**: Store both sample emails from the brief as fixtures in `/fixtures/sample-email-1.txt` and `/fixtures/sample-email-2.txt` for use in E2E and integration tests.
```

---

### **10. README Two-Minute Reviewer Path**

**Priority**: MEDIUM
**Type**: Documentation improvement
**Rationale**: Reduces friction for reviewers/evaluators

#### **Section 19: Handover & Onboarding** (lines 635-640)

**Current Text** (lines 636-637):
```markdown
* **README** with: quick start (dev), architecture overview, endpoints, config, and runbooks.
```

**Replace With**:
```markdown
* **README** with:
  - **Quick Reviewer Path** (2-minute guide for evaluators):
    1. Clone repo: `git clone <repo-url>`
    2. Run local setup: `docker-compose up` (or `dotnet run` if SQLite)
    3. Open Swagger UI: `http://localhost:5001/swagger`
    4. Test sample request (copy-paste from Swagger examples)
    5. View result in Swagger response panel
  - **Developer Quick Start**: Detailed local development setup
  - Architecture overview, endpoints, config, and runbooks
  - Troubleshooting common issues
```

---

### **Additional: Risk & Scope Guardrails**

**Priority**: HIGH
**Type**: Scope/delivery prioritization
**Rationale**: Ensures focus on core functionality before polish

#### **Section 18: Delivery Plan (Milestones)** (lines 622-631)

**Add Before Line 624** (before "M0 (Scaffold & Echo)"):
```markdown
**Priority & Risk Guardrails**

Focus on **M0 → M2** (scaffold, tests, API v1) before investing in UI enhancements or advanced features. This ensures:
- Core parsing, validation, normalization logic is solid and reviewable
- API contract is stable and tested
- Backend functionality can be demonstrated via Swagger (even if UI is minimal)

UI polish (M3), persistence (M4), and deployment (M5) are sequential dependencies; prioritize backend quality over UI aesthetics for handover/review.
```

---

### **Additional: Concrete Implementation Tweaks — Testing Matrix**

**Priority**: MEDIUM
**Type**: Testing documentation enhancement
**Rationale**: Clarifies test coverage strategy

#### **Section 13: TDD/BDD Plan** (lines 483-548)

**Add After Line 547** (after BDD scenarios, before "---"):
```markdown

**Testing Matrix**

| Scenario | Unit Test | Integration Test | E2E Test |
|----------|-----------|------------------|----------|
| Tag overlap validation | ✓ Domain | ✓ API endpoint | - |
| Rounding policy (Banker's) | ✓ TaxCalculator | ✓ Full pipeline | - |
| Tax rate precedence | ✓ Validator | ✓ API endpoint | - |
| Sample Email 1 (XML island) | - | ✓ Parsing + DB | ✓ UI + API |
| Sample Email 2 (Inline tags) | - | ✓ Parsing + DB | ✓ UI + API |
| Unclosed tags error | ✓ TagValidator | ✓ API 400 response | ✓ Error banner |
| Missing total error | ✓ ExpenseProcessor | ✓ API 400 response | ✓ Error banner |

**Legend**: ✓ = Required test coverage, - = Not applicable/needed
```

---

## Implementation Sequence

To minimize conflicts and ensure logical progression, implement changes in this order:

### **Phase 1: High-Priority Architectural Changes**
1. **Section 8**: Storage strategy (SQLite/Postgres)
2. **Section 16**: Deployment model updates
3. **Section 17**: Open Questions (Question 6)
4. **Section 14**: ADR-0001 reference update

### **Phase 2: API Contract Changes**
5. **Section 4.1**: API response structure (separate expense/other)
6. **Section 6**: Frontend JSON viewer note

### **Phase 3: Parsing & Validation Rules**
7. **Section 4.2**: Add overlapping tags, rounding policy, time handling, tax rate precedence

### **Phase 4: Testing Enhancements**
8. **Section 13**: Add all test scenarios, BDD cases, testing matrix, sample emails note

### **Phase 5: Documentation & Future Items**
9. **Section 7**: Swagger examples (post-core) note
10. **Section 19**: Two-minute reviewer path
11. **Section 18**: M0→M2 priority guardrails
12. **Section 20.3**: Rate limiting backlog item

---

## Post-Implementation Checklist

After updating PRD v0.2 → v0.3:

- [ ] Update document version in header from v0.2 to v0.3
- [ ] Add entry to Document History table (Section: Document History)
- [ ] Update ADR-0001 to reflect SQLite/Postgres strategy (deferred per user)
- [ ] Consider new ADR for API response structure decision (deferred per user)
- [ ] Update CLAUDE.md with SQLite testing guidance (future task)
- [ ] Create sample email fixtures in `/fixtures/` directory (implementation phase)
- [ ] Review all cross-references for consistency

---

## Summary of Sections Modified

| Section | Lines (Approx) | Change Summary |
|---------|----------------|----------------|
| **Section 8** | 325-407 | Storage strategy revised (SQLite local, Postgres deployment) |
| **Section 16** | 575-608 | Deployment model updated; migration path added |
| **Section 17** | 609-621 | Question 6 updated (storage) |
| **Section 14** | 551-562 | ADR-0001 reference updated |
| **Section 4.1** | 129-177 | API response structure split (expense OR other) |
| **Section 6** | 211-243 | JSON viewer note (specific responses) |
| **Section 4.2** | 178-193 | Add: overlapping tags, rounding, time, tax rate precedence |
| **Section 13** | 483-548 | Add: tests, BDD scenarios, testing matrix, sample emails |
| **Section 7** | 244-322 | Add: Swagger examples (post-core) |
| **Section 19** | 635-640 | Add: Two-minute reviewer path |
| **Section 18** | 622-631 | Add: M0→M2 priority guardrails |
| **Section 20.3** | 649-656 | Add: Rate limiting backlog |

**Total Sections Modified**: 12
**Estimated Change Volume**: ~250 lines (additions/replacements)

---

## Next Steps

1. **Approve this plan** (or request modifications)
2. **Implement changes** to prd-technical_spec.md (v0.2 → v0.3)
3. **Update ADRs** as needed (deferred until after PRD update per user instruction):
   - ADR-0001: Major revision for SQLite/Postgres strategy
   - New ADR or ADR-0003 update: API response structure decision
4. **Commit changes** using AADF-compliant commit workflow

---

**Document Control**

- **Plan Version**: 1.0
- **Created**: 2025-10-06
- **Last Updated**: 2025-10-06
- **Next Review**: After PRD v0.3 implementation

 Now let me analyze the gaps between the current delivery plan (based on v0.2 ofproject-context/specifications/prd-technical_spec.md ) and PRD v0.3:

  📊 Delivery Plan Analysis: v0.2 → v0.3 Gap Assessment

  🔴 Critical Updates Required

  1. Rounding Policy Change (M1, M2)

  Current Plan (v0.2): "round half away from zero"
  PRD v0.3: Banker's Rounding (MidpointRounding.ToEven)

  Impact:
  - ADR-0009 documents this change comprehensively
  - Unit tests must verify: 2.125 → 2.12 (not 2.13)
  - Tax calculation examples need updating

  ---
  2. Storage Strategy Change (M4)

  Current Plan (v0.2): "Either Postgres everywhere OR SQLite for tests + Postgres for dev/prod"
  PRD v0.3 + ADR-0001-v2: SQLite for local dev/tests, Postgres for deployment (M5+)

  Impact:
  - M4 description needs rewrite to reflect SQLite-first approach
  - Migration path documented (SQLite → Postgres at M5/M6)
  - Testcontainers no longer needed (in-memory SQLite for tests)
  - Faster iteration, zero external dependencies locally

  ---
  3. Parsing Rules Enhancement (M1)

  Missing from v0.2 plan:
  - Overlapping tag validation (not just balance checking)
    - Example: <a><b></a></b> must be rejected
    - Requires stack-based parser, not just regex balance
  - Tax rate precedence (request > config > error/fallback)
    - Strict mode configurable via StrictTaxRate
  - Ambiguous time handling
    - Whitelist: HH:mm, h:mm tt
    - Reject: 230, 2.30 (ambiguous)

  Impact: ADR-0008 documents these rules; M1 test checklist needs expansion

  ---
  4. Response Contract Change (M2)

  Current Plan (v0.2): Not explicitly detailed
  PRD v0.3 + ADR-0007: Expense XOR Other (never both)

  Impact:
  - Response structure must be specific to classification
  - TypeScript discriminated unions for type safety
  - Swagger examples must show both response types separately

  ---
  5. Testing Strategy Expansion (M3, Throughout)

  Missing from v0.2 plan:
  - 6 new BDD scenarios (PRD v0.3 Section 13)
    - Overlapping tags error
    - Banker's Rounding verification
    - Tax rate precedence (3 scenarios)
    - Ambiguous time rejection
  - Sample email fixtures requirement
    - Store emails from test brief as fixtures
    - Use in E2E tests for validation

  Impact: ADR-0010 provides full testing matrix; E2E test coverage needs expansion

  ---
  6. M0→M2 Priority Guardrails (PRD v0.3 Section 18)

  New Guidance:
  "Focus on M0→M2 (scaffold, tests, API v1) before investing in UI enhancements or advanced features. This ensures core parsing, validation, normalization logic is solid and reviewable."

  Impact: Delivery sequence should prioritize:
  1. M0: Scaffold + Echo ✅ (unchanged)
  2. M1: Core parsing with all v0.3 rules 🔴 (needs update)
  3. M2: API contract with Swagger basic ✅ (mostly good)
  4. M3: UI minimal (defer polish) ✅
  5. M4: Persistence (SQLite first) 🔴 (needs rewrite)
  6. M5: Ops + Postgres migration 🔴 (new scope)

  ---
  📋 Detailed Change Recommendations

  M1 Updates Required

  Current:
  * Reject unbalanced or overlapping tags
  * Round half away from zero

  Should be (v0.3):
  * Stack-based tag validation (not just balance):
    - Reject overlapping: `<a><b></a></b>` → UNCLOSED_TAGS
    - Proper nesting only: `<a><b></b></a>` ✅
  * Banker's Rounding (MidpointRounding.ToEven):
    - 2.125 → 2.12 (round to even)
    - Apply at business boundaries (tax calc, totals)
  * Tax rate precedence chain:
    - Request param > Config default > Error (if strict) or 0.15
    - Configurable strict mode via StrictTaxRate
  * Time parsing whitelist:
    - Accept: HH:mm, h:mm tt
    - Reject ambiguous: 230, 2.30 (log warning, return null)

  Test Checklist Addition:
  * ✅ Overlapping tags (not just unclosed): `<a><b></a></b>` → error
  * ✅ Banker's Rounding edge cases: 2.125→2.12, 2.135→2.14, 2.145→2.14
  * ✅ Tax rate precedence: request wins, config fallback, strict mode error
  * ✅ Ambiguous time rejection: "230" → null + warning logged

  ---
  M2 Updates Required

  Current:
  * Response: { classification: "expense"|"other", expense?: {...}, other?: {...} }

  Should be (v0.3 + ADR-0007):
  * Response (Expense):
    {
      "classification": "expense",
      "expense": { vendor, total, totalExclTax, salesTax, ... },
      "meta": { correlationId, warnings, tagsFound }
    }

  * Response (Other):
    {
      "classification": "other",
      "other": { rawTags, note },
      "meta": { correlationId, warnings, tagsFound }
    }

  * NEVER both expense and other in same response (XOR contract)
  * TypeScript: Use discriminated unions for type safety

  Swagger Update:
  * Add OpenAPI `oneOf` discriminator for classification
  * Separate example responses for expense vs other
  * Document both 200 success shapes clearly

  ---
  M4 Complete Rewrite Required

  Current (v0.2):
  ## M4 — Persistence (optional, ½ day if kept)
  * Decision: Either Postgres everywhere (Testcontainers) OR SQLite for tests + Postgres for dev/prod

  Should be (v0.3 + ADR-0001-v2):
  ## M4 — Local Persistence with SQLite (½ day)

  **Purpose:** Fast, hermetic local development and testing with SQLite; prepare for Postgres migration.

  * **Storage Strategy** (ADR-0001-v2):
    - **SQLite file-based** for local dev: `Data Source=flowingly.db`
    - **SQLite in-memory** for tests: `Data Source=:memory:` (hermetic, fast)
    - **No external dependencies**: No Docker, no services required
    - **Postgres migration** planned for M5+ deployment

  * **Schema** (same for SQLite and Postgres):
    - `messages`: id, correlation_id, content, content_hash, classification
    - `expenses`: id, message_id, cost_centre, totals, tax breakdown
    - `other_payloads`: id, message_id, raw_tags (JSON)
    - `processing_logs`: id, message_id, step, level, details

  * **EF Core Configuration**:
    - Use EF migrations (test against both SQLite and Postgres)
    - Abstract Postgres-specific features (JSONB → JSON TEXT)
    - Conditional provider registration (SQLite dev, Postgres prod)

  * **Idempotency**:
    - Unique index on `messages.content_hash` (SHA-256)
    - Retry-safe (same input → same output, no duplicates)

  * **DoD**:
    - EF migrations applied to local SQLite
    - Repository tests green with in-memory SQLite
    - Health check `/api/v1/health` returns DB status
    - README documents: "Local dev uses SQLite (zero setup)"

  ---
  M5 Updates Required

  Current:
  ## M5 — Observability & hardening
  * Serilog, rate-limit, API key

  Should be (v0.3):
  ## M5 — Observability & Production Readiness

  **Purpose:** Production hygiene + Postgres migration readiness.

  * **Observability**:
    - Serilog structured logging with correlation IDs
    - Log levels: Dev (Debug), Prod (Information)
    - Optional: Prometheus metrics endpoint (requests, errors, latency)

  * **Security**:
    - API key middleware (ADR-0006): Dev disabled, Prod required
    - Secure XML parser: DtdProcessing=Prohibit, XmlResolver=null
    - Input size limits: 256KB max payload

  * **Rate Limiting** (optional/backlog):
    - Token bucket per API key/IP
    - Return 429 with Retry-After header

  * **Postgres Migration Path** (ADR-0001-v2):
    - Provision Render Postgres instance
    - Run EF migrations: `dotnet ef database update`
    - Test schema compatibility (SQLite vs Postgres)
    - Update connection string for production
    - Deploy with Postgres connection

  * **DoD**:
    - Correlation IDs in all logs
    - API key middleware active in Production
    - Postgres migration tested and documented
    - Render deployment config (render.yaml) ready

  ---
  M6 Updates Required

  Current:
  * One command: docker compose up

  Should be (v0.3 + PRD Section 19):
  ## M6 — Packaging & Demo

  **Purpose:** Reviewer-friendly handover with 2-minute quick start.

  * **README Two-Minute Reviewer Path** (PRD v0.3 Section 19):
    1. Clone repo: `git clone <repo-url>`
    2. Run local setup:
       - Option A: `docker compose up` (if Docker config provided)
       - Option B: `dotnet run` (SQLite, zero external dependencies)
    3. Open Swagger UI: `http://localhost:5001/swagger`
    4. Test sample request (copy-paste from Swagger examples)
    5. View result in Swagger response panel

  * **Developer Quick Start**:
    - Prerequisites: .NET 8, Node 18+, (optional) Docker
    - Local dev: SQLite auto-created (no setup)
    - Commands: `make dev`, `make test`, `npm run e2e`

  * **Documentation**:
    - README: Quick start, architecture overview, troubleshooting
    - ADRs: All architectural decisions documented (10 ADRs)
    - BUILDLOG.md: Implementation history and context
    - Swagger: Interactive API exploration at `/swagger`

  * **Sample Fixtures** (PRD v0.3 Section 13):
    - `/fixtures/sample-email-1-expense.txt`: Expense email from test brief
    - `/fixtures/sample-email-2-other.txt`: Other email from test brief
    - Used in E2E tests for validation

  * **DoD**:
    - README 2-minute path tested by external reviewer
    - Swagger examples copyable and working
    - Sample fixtures stored and tested
    - All tests green in CI
    - Troubleshooting section covers common issues

  ---
  🎯 Revised Milestone Priorities (M0→M2 Focus)

  Per PRD v0.3 Section 18 guardrails:

  Priority Tier 1 (Core Delivery - M0→M2):
  ✅ M0: Scaffold & Echo (unchanged)
  🔴 M1: Core Parsing (UPDATE: add v0.3 rules)
  🔴 M2: API Contract (UPDATE: XOR responses, Banker's Rounding)

  Priority Tier 2 (MVP Completion - M3→M4):
  ✅ M3: UI Minimal (defer polish, focus on functionality)
  🔴 M4: SQLite Persistence (REWRITE: SQLite-first approach)

  Priority Tier 3 (Production Readiness - M5→M6):
  🔴 M5: Ops + Postgres (UPDATE: migration path)
  🔴 M6: Packaging (UPDATE: 2-minute reviewer path)

  Rationale: Backend quality and correctness (M0→M2) prioritized over UI polish. Ensures core functionality is reviewable via Swagger before investing in frontend enhancements.

  ---
  📝 Updated Test Plan

  Unit (parser) - Add to existing:
  * ✅ Overlapping tags: <a><b></a></b> → UNCLOSED_TAGS (not just balance)
  * ✅ Banker's Rounding: 2.125→2.12, 2.135→2.14, 2.145→2.14, 2.155→2.16
  * ✅ Tax rate: request=0.10 + config=0.15 → use 0.10 (request wins)
  * ✅ Tax rate: no request + config=0.15 → use 0.15 (config default)
  * ✅ Tax rate: no request + no config + strict=true → error MISSING_TAXRATE
  * ✅ Ambiguous time: "230" → null + warning logged
  * ✅ Valid time: "14:30" → TimeSpan(14,30,0)

  E2E (Playwright) - Add to existing:
  * Sample Email 1 (from test brief): Submit → verify expense extraction, tax calc
  * Sample Email 2 (from test brief): Submit → verify other/unprocessed handling
  * Overlapping tags: Submit → error banner with UNCLOSED_TAGS code
  * Tax calculation: Verify Banker's Rounding in displayed results

  ---
  🔄 Engineering Decisions to Update

  Current (v0.2):
  * Money & rounding: half away from zero, 2dp
  * DB stance: Postgres everywhere OR SQLite for tests

  Should be (v0.3):
  * Money & rounding: Banker's Rounding (MidpointRounding.ToEven), 2dp
    - Rationale: Statistically unbiased, financial industry standard
    - Document: ADR-0009

  * DB stance: SQLite local/test, Postgres deployment (M5+)
    - Rationale: Fast iteration, zero setup friction, proven migration path
    - Document: ADR-0001-v2

  * Tag validation: Stack-based nesting validation (not regex balance)
    - Rationale: Detect overlapping tags, not just unclosed
    - Document: ADR-0008

  * Response contract: Classification-specific (expense XOR other)
    - Rationale: Type safety, smaller payloads, clear contracts
    - Document: ADR-0007

  ---
  🎯 Alignment with Test Requirements

  Based on the original test PDF, the key requirements are:

  ✅ Correctly Covered by Updated Plan

  1. Parse expense emails with inline tags and XML islands → M1, M2
  2. Compute GST breakdown (tax-inclusive → exclusive + tax) → M2 (with Banker's Rounding)
  3. Validate tag integrity (reject malformed) → M1 (stack-based validator)
  4. Handle missing fields (default cost_centre to UNKNOWN) → M1
  5. React UI for text submission and result display → M3
  6. Error handling with clear codes → M2 (400 with error.code)
  7. Testing (unit, integration, E2E) → All milestones (expanded in v0.3)
  8. Documentation (README, ADRs, Swagger) → M6 (enhanced 2-minute path)

  🔴 Gaps Identified and Addressed

  1. Sample Email Fixtures → Added to M6 (store emails from brief, use in E2E)
  2. Banker's Rounding → Added to M1, M2 (was "half away from zero")
  3. Overlapping Tag Detection → Added to M1 (was just "unbalanced")
  4. Response Contract Clarity → Added to M2 (expense XOR other, not combined)
  5. Storage Strategy Clarity → Fixed in M4 (SQLite-first, not ambiguous)

  ---
  📊 Summary of Required Updates

  | Milestone             | Change Type | Priority | Effort                               |
  |-----------------------|-------------|----------|--------------------------------------|
  | M1                    | 🔴 Update   | HIGH     | 30 min (add v0.3 rules to checklist) |
  | M2                    | 🔴 Update   | HIGH     | 20 min (clarify response contract)   |
  | M4                    | 🔴 Rewrite  | HIGH     | 45 min (SQLite-first strategy)       |
  | M5                    | 🔴 Update   | MEDIUM   | 30 min (add Postgres migration)      |
  | M6                    | 🔴 Update   | MEDIUM   | 20 min (2-minute reviewer path)      |
  | Test Plan             | 🔴 Expand   | HIGH     | 15 min (add 6 BDD scenarios)         |
  | Engineering Decisions | 🔴 Update   | HIGH     | 10 min (rounding + DB stance)        |

  Total Effort: ~3 hours to update delivery plan comprehensively

  ---
  ✅ Recommendation

  Update the delivery plan to align with PRD v0.3 before implementation begins. The changes are critical for correctness (Banker's Rounding, overlapping tags, response contract) and
  efficiency (SQLite-first approach).
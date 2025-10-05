
# Goal (single sentence)

Based on project-context/specifications/prd-technical_spec.md (Flowingly Parsing Service — PRD + Technical Specification (v0.3)), deliver a small, correct, well-tested **.NET Web API + React UI** that parses the provided "receipt-like" text, validates tags, computes **GST-exclusive total** from an **inclusive** `<total>`, and surfaces clear errors — with a crisp README and one-click run.

---

# Delivery plan (M0 → M6)

## M0 — Scaffold & Echo (same day)

**Purpose:** Prove the end-to-end wiring (repo, API, UI, CI smoke).

* **API**: `POST /api/v1/parse` accepts `{ text, taxRate? }` and returns `"Processed receipt Input: [text]"` + a generated `correlation_id`.
* **UI**: Textarea + “Send” → shows API echo.
* **Infra**: Solution + projects; basic Serilog; Swagger on `/swagger`; CORS open for `localhost`.
* **CI**: Build + unit test + lint on PR.
* **Definition of done (DoD)**

  * `docker compose up` brings API+UI
  * Paste text → see echo with correlation id
  * README quick-start exists

## M1 — Core parsing & validation (1 day)

**Purpose:** Lock the rules that the brief will grade you on.

* **Rules (lock):**

  * **Stack-based tag validation** (not just balance checking):
    * Reject **overlapping** tags: `<a><b></a></b>` → `UNCLOSED_TAGS`
    * Accept **proper nesting** only: `<a><b></b></a>` ✅
  * If `<total>` **missing**: `MISSING_TOTAL`.
  * If `<cost_centre>` missing: default to `"UNKNOWN"`.
  * Prefer `<expense>`-scoped `<total>` over global if both exist.
  * **Number normalization**: strip currency symbols/commas; use **decimal(18,2)** for money.
  * **Banker's Rounding** (MidpointRounding.ToEven) at business boundaries:
    * 2.125 → 2.12 (round to even)
    * Apply to tax calculations and totals
  * **Tax rate precedence chain**:
    * Request param > Config default > Error (if strict) or 0.15
    * Configurable strict mode via `StrictTaxRate`
  * **Time parsing whitelist**:
    * Accept: `HH:mm`, `h:mm tt`
    * Reject ambiguous: `230`, `2.30` (log warning, return null)
* **Parser**: Pure function(s) with unit tests (happy + edge + failure).
* **DoD**

  * Unit tests for: missing `<total>`, default cost centre, overlapping tags (not just unclosed), currency/commas, precedence.
  * ✅ Overlapping tags: `<a><b></a></b>` → `UNCLOSED_TAGS`
  * ✅ Banker's Rounding edge cases: 2.125→2.12, 2.135→2.14, 2.145→2.14
  * ✅ Tax rate precedence: request wins, config fallback, strict mode error
  * ✅ Ambiguous time rejection: `"230"` → null + warning logged

## M2 — Domain + API contract (½–1 day)

**Purpose:** Wrap parsing in the API surface exactly as spec'd.

* **Contract**

  * Request: `{ text: string, taxRate?: number }` (if absent → default 0.15).
  * **Response (Expense)** — classification-specific (XOR contract):
    ```json
    {
      "classification": "expense",
      "expense": {
        "vendor": "...",
        "total": 115.00,
        "totalExclTax": 100.00,
        "salesTax": 15.00,
        "costCentre": "DEP-001"
      },
      "meta": {
        "correlationId": "...",
        "warnings": [],
        "tagsFound": ["vendor", "total", "cost_centre"]
      }
    }
    ```
  * **Response (Other)** — classification-specific (XOR contract):
    ```json
    {
      "classification": "other",
      "other": {
        "rawTags": { "reservation": "...", "guest_count": "..." },
        "note": "Unprocessed content stored for future processing"
      },
      "meta": {
        "correlationId": "...",
        "warnings": [],
        "tagsFound": ["reservation", "guest_count"]
      }
    }
    ```
  * **Response (error)**: `{ error: { code, message, details? }, correlation_id }` with `400`.
  * **NEVER** return both `expense` and `other` in same response (XOR enforcement).
* **Tax math** (with Banker's Rounding)

  * Inputs are **tax-inclusive** totals. Compute:

    * `total_excl_tax = total_incl / (1 + rate)`
    * `tax = total_incl - total_excl_tax`
    * Round result to 2dp using **Banker's Rounding** (MidpointRounding.ToEven).
* **Swagger**: Add **concrete examples** (happy path + each error).
  * Use OpenAPI `oneOf` discriminator for `classification`
  * Separate example responses for expense vs other
  * Document both 200 success shapes clearly
* **DoD**

  * Contract tests (WebApplicationFactory) verify codes + payload shape
  * Swagger examples visible and copy-pastable
  * TypeScript discriminated unions for type safety in client

## M3 — UI v1 (½ day)

**Purpose:** Minimal, crisp React front that demonstrates the flow clearly.

* Components:

  * Textarea (with label), “Parse” button, “Clear” button
  * JSON viewer for response; error callout showing `error.code` + `message`
* Accessibility: labels, keyboard focus, basic contrast.
* **Playwright E2E**

  * Happy path: sample text → parsed JSON w/ computed tax + total_excl
  * Failure: missing `<total>` → shows 400 message
  * Failure: unbalanced tags → shows 400 message
* **DoD**

  * `npm run e2e` green locally and under CI (headed in CI optional)

## M4 — Local Persistence with SQLite (½ day)

**Purpose:** Fast, hermetic local development and testing with SQLite; prepare for Postgres migration.

* **Storage Strategy** (ADR-0001-v2):
  * **SQLite file-based** for local dev: `Data Source=flowingly.db`
  * **SQLite in-memory** for tests: `Data Source=:memory:` (hermetic, fast)
  * **No external dependencies**: No Docker, no services required
  * **Postgres migration** planned for M5+ deployment

* **Schema** (same for SQLite and Postgres):
  * `messages`: id, correlation_id, content, content_hash, classification
  * `expenses`: id, message_id, cost_centre, totals, tax breakdown
  * `other_payloads`: id, message_id, raw_tags (JSON)
  * `processing_logs`: id, message_id, step, level, details

* **EF Core Configuration**:
  * Use EF migrations (test against both SQLite and Postgres)
  * Abstract Postgres-specific features (JSONB → JSON TEXT)
  * Conditional provider registration (SQLite dev, Postgres prod)

* **Idempotency**:
  * Unique index on `messages.content_hash` (SHA-256)
  * Retry-safe (same input → same output, no duplicates)

* **DoD**:
  * EF migrations applied to local SQLite
  * Repository tests green with in-memory SQLite
  * Health check `/api/v1/health` returns DB status
  * README documents: "Local dev uses SQLite (zero setup)"

## M5 — Observability & Production Readiness (½ day)

**Purpose:** Production hygiene + Postgres migration readiness.

* **Observability**:
  * Serilog structured logging with correlation IDs
  * Log levels: Dev (Debug), Prod (Information)
  * Optional: Prometheus metrics endpoint (requests, errors, latency)

* **Security**:
  * API key middleware (ADR-0006): Dev disabled, Prod required
  * Secure XML parser: DtdProcessing=Prohibit, XmlResolver=null
  * Input size limits: 256KB max payload

* **Rate Limiting** (optional/backlog):
  * Token bucket per API key/IP
  * Return 429 with Retry-After header

* **Postgres Migration Path** (ADR-0001-v2):
  * Provision Render Postgres instance
  * Run EF migrations: `dotnet ef database update`
  * Test schema compatibility (SQLite vs Postgres)
  * Update connection string for production
  * Deploy with Postgres connection

* **DoD**:
  * Correlation IDs in all logs
  * API key middleware active in Production
  * Postgres migration tested and documented
  * Render deployment config (render.yaml) ready

## M6 — Packaging & Demo (½ day)

**Purpose:** Reviewer-friendly handover with 2-minute quick start.

* **README Two-Minute Reviewer Path** (PRD v0.3 Section 19):
  1. Clone repo: `git clone <repo-url>`
  2. Run local setup:
     * Option A: `docker compose up` (if Docker config provided)
     * Option B: `dotnet run` (SQLite, zero external dependencies)
  3. Open Swagger UI: `http://localhost:5001/swagger`
  4. Test sample request (copy-paste from Swagger examples)
  5. View result in Swagger response panel

* **Developer Quick Start**:
  * Prerequisites: .NET 8, Node 18+, (optional) Docker
  * Local dev: SQLite auto-created (no setup)
  * Commands: `make dev`, `make test`, `npm run e2e`

* **Documentation**:
  * README: Quick start, architecture overview, troubleshooting
  * ADRs: All architectural decisions documented (10 ADRs)
  * BUILDLOG.md: Implementation history and context
  * Swagger: Interactive API exploration at `/swagger`

* **Sample Fixtures** (PRD v0.3 Section 13):
  * `/fixtures/sample-email-1-expense.txt`: Expense email from test brief
  * `/fixtures/sample-email-2-other.txt`: Other email from test brief
  * Used in E2E tests for validation

* **DoD**:
  * README 2-minute path tested by external reviewer
  * Swagger examples copyable and working
  * Sample fixtures stored and tested
  * All tests green in CI
  * Troubleshooting section covers common issues

---

# Test plan (what we actually verify)

**Unit (parser):**

* ✅ Overlapping tags (not just unclosed): `<a><b></a></b>` → `UNCLOSED_TAGS`
* ✅ Missing `<total>` → `MISSING_TOTAL`
* ✅ Missing `<cost_centre>` → `"UNKNOWN"`
* ✅ Currency + commas: `<total>$35,000.00</total>` → `35000.00`
* ✅ Two totals (global vs expense) with different values → **expense wins**
* ✅ Banker's Rounding: `2.125`→`2.12`, `2.135`→`2.14`, `2.145`→`2.14`, `2.155`→`2.16`
* ✅ Tax rate precedence:
  * Request `taxRate=0.10` + config `0.15` → use `0.10` (request wins)
  * No request + config `0.15` → use `0.15` (config default)
  * No request + no config + `strict=true` → error `MISSING_TAXRATE`
* ✅ Ambiguous time rejection: `"230"` → null + warning logged
* ✅ Valid time parsing: `"14:30"` → `TimeSpan(14,30,0)`

**Contract/integration:**

* `400` codes map to correct `{ error: { code, message } }` payload
* `taxRate` precedence (request > config default (0.15))
* Correlation id always present
* Response contract: expense XOR other (never both)

**E2E (Playwright):**

* Happy path: paste sample, click "Parse", shows computed values with Banker's Rounding
* Sample Email 1 (from test brief): Submit → verify expense extraction, tax calc
* Sample Email 2 (from test brief): Submit → verify other/unprocessed handling
* Missing total: UI surfaces error message with `MISSING_TOTAL` code
* Overlapping tags: UI surfaces error banner with `UNCLOSED_TAGS` code

---

# Repo layout (concise)

```
/ (mono repo)
  api/
    src/Flowingly.ParsingService.Api
    src/Flowingly.ParsingService.Domain
    tests/Flowingly.ParsingService.Tests (unit+contract)
    Flowingly.ParsingService.sln
  ui/
    src/ (Vite + React)
    e2e/ (Playwright)
  docker-compose.yml (api, ui, db if used)
  Makefile
  README.md
```

---

# Engineering decisions to lock early

* **Money & rounding**: Decimal math, **Banker's Rounding** (MidpointRounding.ToEven), 2dp output.
  * Rationale: Statistically unbiased, financial industry standard
  * Document: ADR-0009
* **Tag validator**: **Stack-based nesting validation** (not regex balance).
  * Rationale: Detect overlapping tags, not just unclosed
  * Document: ADR-0008
* **Tax source of truth**: Request `taxRate` > config default (0.15).
  * Configurable strict mode via `StrictTaxRate`
* **DB stance**: **SQLite local/test, Postgres deployment (M5+)**.
  * Rationale: Fast iteration, zero setup friction, proven migration path
  * Document: ADR-0001-v2
* **Response contract**: **Classification-specific (expense XOR other)**.
  * Rationale: Type safety, smaller payloads, clear contracts
  * Document: ADR-0007
* **Security**:

  * XML parser hardened (no DTD, no external entities, size/time caps)
  * API key middleware (dev disabled, prod required)
* **Non-goals (v1)**: No fuzzy NER, no multi-currency, no time parsing beyond a whitelisted format; no reservation logic beyond "other".

---

# What to implement first (sequence inside each milestone)

**Priority Tier 1 (Core Delivery — M0→M2 Focus per PRD v0.3 Section 18):**

1. **M0:** Start from the **README** stub + docker compose; wire echo → UI.
2. **M1:** Finish **parser** as pure lib with unit tests (all v0.3 rules: stack-based validation, Banker's Rounding, tax precedence, time whitelist).
3. **M2:** Wrap in API handlers; lock error contract with XOR responses; add Swagger **examples** (separate for expense/other).

**Priority Tier 2 (MVP Completion — M3→M4):**

4. **M3:** UI + Playwright (defer polish, focus on functionality).
5. **M4:** SQLite persistence (file-based local dev, in-memory tests).

**Priority Tier 3 (Production Readiness — M5→M6):**

6. **M5:** Serilog with correlation IDs; Postgres migration path; security hardening.
7. **M6:** README 2-minute reviewer path; sample fixtures; demo script.

**Rationale:** Backend quality and correctness (M0→M2) prioritized over UI polish. Ensures core parsing, validation, normalization logic is solid and reviewable via Swagger before investing in frontend enhancements.

---

# Acceptance checklist (use this as your final PR gate)

* [ ] `docker compose up` yields working **UI** and **Swagger**
* [ ] Paste the **provided sample** → correct JSON with **GST** and **total_excl**
* [ ] Invalid/overlapping tags → **400** with `UNCLOSED_TAGS`
* [ ] Missing `<total>` → **400** `MISSING_TOTAL`
* [ ] Missing `<cost_centre>` → `"UNKNOWN"` in response
* [ ] Swagger shows concrete request/response examples for happy + each error
* [ ] Unit, contract, and E2E tests: **all green** in CI
* [ ] README “two-minute path” complete

---

# Nice-to-have (only if time allows)

* Simple **rate limiting** → return `429` with retry hint.
* `/metrics` for Prometheus (requests, 4xx/5xx counts, latency).
* Toggle `?includeOther=true` to include “other” block (default false).


# Optimized Delivery Plan — Test Brief Focused

**Optimization Goal:** Meet all test requirements in the fastest, most reliable path. Focus on correctness, testing, and code quality over production features not evaluated by the test brief.

**Key Insight:** The test brief evaluates parsing correctness, testing quality, UX, and code quality — NOT persistence, deployment, or observability. Optimize accordingly.

---

## Goal (single sentence)

Deliver a **correct, well-tested .NET Web API + React UI** that parses expense emails, validates tags, computes GST breakdowns, and demonstrates quality through comprehensive testing and clean code — submittable after **Phase 1 (M0→M3)** in 2-3 days.

---

## Delivery Strategy

### **Phase 1: Core Submission (M0→M3)** — 2-3 days ✅ SUBMITTABLE
Meet all test requirements. Reviewer can clone, run, and evaluate.

### **Phase 2: Polish & Enhancement (M6→M4)** — 1-2 days (optional)
Add professional polish, documentation, and nice-to-haves only if time permits.

### **Phase 3: Production Ready (M5)** — Stretch goal
Production features not required by test brief.

---

# Phase 1: Core Submission (M0→M3)

## M0 — Minimal Scaffold (4 hours, same day)

**Purpose:** Prove end-to-end wiring with absolute minimum setup friction.

### API (.NET 8 Minimal API)
* **Endpoint**: `POST /api/v1/parse` accepts `{ text: string, taxRate?: number }`
* **Response**: Echo with `{ message: "Echo: [text]", correlationId: "..." }`
* **Setup**:
  * .NET solution with `Api` and `Domain` projects
  * Basic Serilog console logging
  * CORS enabled for localhost

### UI (React + Vite)
* **Single page**: Textarea + "Parse" button
* **Display**: Shows API response in `<pre>` tag
* **Minimal styling**: Basic layout, no frameworks needed

### Infrastructure
* **Run command**: `dotnet run --project src/Api` (API on :5001)
* **Run command**: `npm run dev` (UI on :5173)
* **No Docker required** — Simplest possible reviewer setup
* **Basic README**: Clone, run API, run UI (3 steps max)

### DoD
* [ ] Reviewer can run `dotnet run` and `npm run dev` without any setup
* [ ] Paste text in UI → see echo response with correlation ID
* [ ] README has 3-step quick start (clone, run API, run UI)
* [ ] Zero external dependencies (no Docker, no DB)

**Time estimate: 4 hours**

---

## M1 — Core Parsing & Validation (1 day)

**Purpose:** Lock ALL parsing rules that the test brief will evaluate. This is the heart of the submission.

### Parsing Rules (Implemented & Tested)

#### Tag Validation
* **Stack-based validator** (not just regex):
  * Reject overlapping: `<a><b></a></b>` → `UNCLOSED_TAGS`
  * Reject unclosed: `<a><b>` → `UNCLOSED_TAGS`
  * Accept proper nesting: `<a><b></b></a>` ✅
* **Implementation**: Stack-based parser tracking open/close tags

#### Required Fields
* If `<total>` missing → `MISSING_TOTAL` error
* If `<cost_centre>` missing → default to `"UNKNOWN"` (no error)

#### Tag Precedence
* Prefer `<expense>`-scoped `<total>` over document-level if both exist
* Example: `<expense><total>100</total></expense>` vs `<total>200</total>` → use 100

#### Number Normalization
* Strip currency symbols: `$`, `£`, `€`, `NZD`, etc.
* Strip commas: `35,000.00` → `35000.00`
* Use `decimal` type (precision for money)
* Example: `<total>$35,000.00</total>` → `35000.00m`

#### Banker's Rounding (Critical!)
* Use `MidpointRounding.ToEven` at business boundaries
* Examples:
  * `2.125` → `2.12` (round to even)
  * `2.135` → `2.14` (round to even)
  * `2.145` → `2.14` (round to even)
  * `2.155` → `2.16` (round to even)
* Apply to: GST calculations, totals in response

#### GST Tax Calculation
* Input: Tax-inclusive total
* Formula:
  ```
  totalExclTax = totalIncl / (1 + taxRate)
  salesTax = totalIncl - totalExclTax
  ```
* Default tax rate: `0.15` (15% NZ GST)
* Round both values using Banker's Rounding to 2dp

#### Tax Rate Precedence
* Request parameter > Config default (0.15)
* Example: Request has `taxRate: 0.10` → use 0.10, not config

#### Time Parsing (Whitelist)
* Accept: `HH:mm` (24-hour), `h:mm tt` (12-hour with AM/PM)
* Reject ambiguous: `230`, `2.30` → log warning, return `null`
* Examples:
  * `"14:30"` ✅ → `TimeSpan(14, 30, 0)`
  * `"2:30 PM"` ✅ → `TimeSpan(14, 30, 0)`
  * `"230"` ❌ → `null` + warning logged

### Test Brief Sample Fixtures
* **Create `/fixtures` directory** at repo root
* **Add sample emails from test brief**:
  * `sample-email-1-expense.txt` — Expense email
  * `sample-email-2-reservation.txt` — Reservation email (other)
* **Use in unit tests** to validate against actual brief examples

### Parser Architecture
* **Pure functions** in `Domain` project
* **No dependencies** on infrastructure (EF, HTTP, etc.)
* **Interfaces**:
  * `ITagValidator` — Tag integrity validation
  * `IXmlIslandExtractor` — Extract XML from text
  * `INumberNormalizer` — Currency/comma normalization
  * `ITaxCalculator` — GST computation
  * `ITimeParser` — Time parsing with whitelist

### Unit Tests (xUnit + FluentAssertions)

**Tag Validation:**
* ✅ Overlapping tags: `<a><b></a></b>` → `UNCLOSED_TAGS`
* ✅ Unclosed tags: `<a><b>` → `UNCLOSED_TAGS`
* ✅ Proper nesting: `<a><b></b></a>` → valid
* ✅ Self-closing: `<br/>` → valid (if supported)

**Required Fields:**
* ✅ Missing `<total>` → `MISSING_TOTAL`
* ✅ Missing `<cost_centre>` → defaults to `"UNKNOWN"`
* ✅ Both present → uses actual cost_centre value

**Normalization:**
* ✅ Currency symbols: `$35,000.00` → `35000.00`
* ✅ Commas only: `1,234.56` → `1234.56`
* ✅ Mixed: `NZD 1,234.56` → `1234.56`

**Banker's Rounding:**
* ✅ `2.125` → `2.12`
* ✅ `2.135` → `2.14`
* ✅ `2.145` → `2.14`
* ✅ `2.155` → `2.16`
* ✅ Edge cases: `0.005`, `0.015`, `0.025`

**GST Calculation:**
* ✅ `$115.00` @ 15% → `totalExclTax: $100.00`, `salesTax: $15.00`
* ✅ `$100.00` @ 15% → `totalExclTax: $86.96`, `salesTax: $13.04`
* ✅ Verify Banker's Rounding applied

**Tax Rate Precedence:**
* ✅ Request `taxRate=0.10` + config `0.15` → use `0.10`
* ✅ No request + config `0.15` → use `0.15`

**Time Parsing:**
* ✅ `"14:30"` → `TimeSpan(14, 30, 0)`
* ✅ `"2:30 PM"` → `TimeSpan(14, 30, 0)`
* ✅ `"230"` → `null` + warning logged
* ✅ `"2.30"` → `null` + warning logged

**Test Brief Samples:**
* ✅ Sample email 1 (expense) → correct extraction
* ✅ Sample email 2 (reservation) → classified as "other"

### DoD
* [ ] All parsing rules implemented as pure functions
* [ ] 30+ unit tests covering happy path + edge cases + failures
* [ ] Test fixtures from brief included and tested
* [ ] All tests green (100% pass rate)
* [ ] Code coverage >80% on parser logic
* [ ] Zero dependencies on DB or HTTP

**Time estimate: 1 day (8 hours)**

---

## M2 — API Contract & Error Handling (½ day, 4 hours)

**Purpose:** Wrap parser in HTTP API with correct contracts and error handling.

### Request Contract
```json
{
  "text": "Hi Joshua,\n\n<expense><total>$35,000.00</total>...",
  "taxRate": 0.15  // Optional, defaults to 0.15 if absent
}
```

### Response Contract (Expense)
```json
{
  "classification": "expense",
  "expense": {
    "vendor": "Awesome Expense Vendor",
    "total": 35000.00,
    "totalExclTax": 30434.78,
    "salesTax": 4565.22,
    "costCentre": "DEP-001",
    "description": "Expense for stuff and things"
  },
  "meta": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "warnings": []
  }
}
```

### Response Contract (Other)
```json
{
  "classification": "other",
  "other": {
    "rawTags": {
      "reservation": "RES-12345",
      "guest_count": "4"
    },
    "note": "Unprocessed content stored for future processing"
  },
  "meta": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "warnings": []
  }
}
```

### Error Response Contract
```json
{
  "error": {
    "code": "MISSING_TOTAL",
    "message": "The <total> tag is required for expense processing",
    "details": {
      "field": "total",
      "tagsFound": ["vendor", "cost_centre"]
    }
  },
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Error Codes
* `UNCLOSED_TAGS` — Tag validation failed (overlapping or unclosed)
* `MISSING_TOTAL` — No `<total>` tag found in expense content
* `INVALID_FORMAT` — Malformed XML or invalid structure
* `INVALID_TAX_RATE` — Tax rate not in valid range (0-1)

### API Implementation
* **Minimal API** endpoints in `Api` project
* **Handler pattern**: Endpoint → Handler → Parser → Response builder
* **FluentValidation**: Request validation (tax rate range, text not empty)
* **Correlation ID**: Generate GUID for each request, include in all logs/responses
* **Error mapping**: Domain errors → HTTP 400 with error codes

### Contract Tests (WebApplicationFactory)
* ✅ Happy path: Valid expense → 200 with expense response
* ✅ Happy path: Valid reservation → 200 with other response
* ✅ Error: Missing `<total>` → 400 with `MISSING_TOTAL`
* ✅ Error: Overlapping tags → 400 with `UNCLOSED_TAGS`
* ✅ Error: Invalid tax rate (1.5) → 400 with `INVALID_TAX_RATE`
* ✅ Tax rate precedence: Request param overrides config
* ✅ Correlation ID present in all responses (success + error)
* ✅ Response contract: Never both `expense` and `other` (XOR enforcement)

### DoD
* [ ] API accepts request contract, returns correct response
* [ ] All error scenarios return 400 with error codes
* [ ] Contract tests green (WebApplicationFactory)
* [ ] Correlation ID in all responses and logs
* [ ] Tax rate from request used if provided
* [ ] FluentValidation on request payload

**Time estimate: 4 hours**

---

## M3 — Minimal UI & E2E Tests (½ day, 4 hours)

**Purpose:** Demonstrate working end-to-end flow with simple, functional UI and automated E2E tests.

### UI Implementation (React + Vite)

**Minimal Components:**
```tsx
<ParserApp>
  <textarea> // For input text
  <button>Parse</button>
  <button>Clear</button>
  <ResponseDisplay> // Shows JSON result or error
    <pre>{JSON.stringify(response, null, 2)}</pre>
    {error && <ErrorBanner code={error.code} message={error.message} />}
  </ResponseDisplay>
</ParserApp>
```

**Features:**
* Textarea with label "Paste email text here"
* Parse button (calls API)
* Clear button (resets form)
* Response display:
  * **Success**: Green border + formatted JSON
  * **Error**: Red border + error code + message
* Loading state while API request in flight

**No fancy UI libraries** — Use plain React + CSS for speed
* Basic CSS for layout (flexbox)
* Simple color coding (green/red for success/error)
* Focus on functionality, not polish

**API Client:**
```typescript
// src/api/parseClient.ts
export async function parseText(text: string, taxRate?: number) {
  const response = await fetch('http://localhost:5001/api/v1/parse', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text, taxRate })
  });
  return response.json();
}
```

### E2E Tests (Playwright)

**Setup:**
* `npm install -D @playwright/test`
* API and UI both running during tests
* Use `playwright.config.ts` with base URL

**Test Scenarios:**

**Happy Path (Expense):**
```typescript
test('should parse expense email and show GST breakdown', async ({ page }) => {
  // Read fixture
  const expenseEmail = await readFile('fixtures/sample-email-1-expense.txt');

  // Navigate and interact
  await page.goto('/');
  await page.fill('textarea', expenseEmail);
  await page.click('button:has-text("Parse")');

  // Verify response
  await expect(page.locator('.response-display')).toContainText('expense');
  await expect(page.locator('.response-display')).toContainText('totalExclTax');
  await expect(page.locator('.response-display')).toContainText('salesTax');

  // Verify green border (success)
  await expect(page.locator('.response-display')).toHaveCSS('border-color', 'green');
});
```

**Happy Path (Other):**
```typescript
test('should classify reservation as other', async ({ page }) => {
  const reservationEmail = await readFile('fixtures/sample-email-2-reservation.txt');

  await page.goto('/');
  await page.fill('textarea', reservationEmail);
  await page.click('button:has-text("Parse")');

  await expect(page.locator('.response-display')).toContainText('other');
  await expect(page.locator('.response-display')).toContainText('reservation');
});
```

**Error: Missing Total:**
```typescript
test('should show MISSING_TOTAL error', async ({ page }) => {
  const invalidText = '<expense><vendor>Test</vendor></expense>';

  await page.goto('/');
  await page.fill('textarea', invalidText);
  await page.click('button:has-text("Parse")');

  await expect(page.locator('.error-banner')).toContainText('MISSING_TOTAL');
  await expect(page.locator('.response-display')).toHaveCSS('border-color', 'red');
});
```

**Error: Overlapping Tags:**
```typescript
test('should show UNCLOSED_TAGS error for overlapping tags', async ({ page }) => {
  const invalidText = '<expense><a><b></a></b><total>100</total></expense>';

  await page.goto('/');
  await page.fill('textarea', invalidText);
  await page.click('button:has-text("Parse")');

  await expect(page.locator('.error-banner')).toContainText('UNCLOSED_TAGS');
});
```

**GST Calculation Verification:**
```typescript
test('should calculate GST with Bankers Rounding', async ({ page }) => {
  const text = '<expense><total>$115.00</total><vendor>Test</vendor></expense>';

  await page.goto('/');
  await page.fill('textarea', text);
  await page.click('button:has-text("Parse")');

  // Verify Banker's Rounding applied
  const response = await page.locator('.response-display').textContent();
  expect(response).toContain('"totalExclTax": 100.00');
  expect(response).toContain('"salesTax": 15.00');
});
```

### DoD
* [ ] UI shows textarea, parse button, clear button
* [ ] API call works (displays response or error)
* [ ] Success: Green border + formatted JSON
* [ ] Error: Red border + error code + message
* [ ] 5 E2E tests green (happy paths + errors)
* [ ] Tests use sample fixtures from test brief
* [ ] `npm run e2e` command works locally

**Time estimate: 4 hours**

---

## ✅ Phase 1 Complete — READY TO SUBMIT

**Total Time: 2-3 days (20 hours)**

**At this point you have:**
* ✅ Full parsing logic with all validation rules
* ✅ GST calculation (Banker's Rounding)
* ✅ .NET Web API with correct contracts
* ✅ React UI (functional, tested)
* ✅ 30+ unit tests
* ✅ 10+ contract tests
* ✅ 5+ E2E tests
* ✅ Sample fixtures from test brief
* ✅ README with clone + run instructions
* ✅ Correlation IDs in all responses
* ✅ Error handling with clear codes
* ✅ Zero external dependencies (no Docker, no DB)

**Reviewer can:**
1. `git clone <repo>`
2. `dotnet run --project src/Api`
3. `npm run dev` (in ui/)
4. Paste sample email → see results
5. Run tests: `dotnet test && npm run e2e`

**This meets 100% of test brief requirements.**

---

# Phase 2: Polish & Enhancement (Optional)

**Only pursue if you have 1-2 days remaining after Phase 1.**

---

## M6 — Documentation & Developer Experience (½ day, 4 hours)

**Purpose:** Professional polish for documentation and ease of review.

### Enhanced README

**Two-Minute Reviewer Path:**
```markdown
## Quick Start (2 minutes)

### Prerequisites
- .NET 8 SDK
- Node 18+

### Run Locally
1. Clone: `git clone <repo-url>`
2. Start API:
   ```bash
   cd api
   dotnet run --project src/Flowingly.ParsingService.Api
   # API running on http://localhost:5001
   ```
3. Start UI:
   ```bash
   cd ui
   npm install
   npm run dev
   # UI running on http://localhost:5173
   ```
4. Open browser: http://localhost:5173
5. Paste sample email (see `/fixtures/sample-email-1-expense.txt`)
6. Click "Parse" → See GST breakdown

### Run Tests
```bash
# Unit + Contract tests
cd api
dotnet test

# E2E tests
cd ui
npm run e2e
```

### Swagger UI (Optional)
- API documentation: http://localhost:5001/swagger
- Try it out with examples
```

**Developer Guide:**
* Architecture overview (Clean/Hexagonal)
* Project structure
* Key design decisions (link to ADRs)
* Testing strategy

**Troubleshooting:**
* Port conflicts (5001, 5173)
* CORS issues
* Common errors

### Swagger UI Setup
* **Swashbuckle** NuGet package
* **OpenAPI spec** at `/swagger/v1/swagger.json`
* **Swagger UI** at `/swagger`
* **Examples** for request/response (both success and errors)
* **oneOf discriminator** for expense vs other responses

### Docker Compose (Optional)
```yaml
version: '3.8'
services:
  api:
    build: ./api
    ports:
      - "5001:5001"
  ui:
    build: ./ui
    ports:
      - "5173:5173"
    depends_on:
      - api
```

**Benefit:** One-command setup for reviewers who prefer Docker

### Sample Fixtures Documentation
* **README in `/fixtures`** explaining each sample
* **Expected outputs** documented
* **Usage in tests** explained

### Makefile / Scripts
```makefile
.PHONY: dev test clean

dev:
	@echo "Starting API..."
	cd api && dotnet run --project src/Flowingly.ParsingService.Api &
	@echo "Starting UI..."
	cd ui && npm run dev

test:
	@echo "Running unit tests..."
	cd api && dotnet test
	@echo "Running E2E tests..."
	cd ui && npm run e2e

clean:
	cd api && dotnet clean
	cd ui && rm -rf node_modules dist
```

### DoD
* [ ] README has 2-minute quick start (tested by external person)
* [ ] Swagger UI working at `/swagger`
* [ ] Swagger has concrete examples for all scenarios
* [ ] Docker compose works (optional)
* [ ] Makefile commands work
* [ ] Troubleshooting section complete

**Time estimate: 4 hours**

---

## M4 — Persistence (Optional, ½ day, 4 hours)

**Purpose:** Add audit trail for requests/responses. **Only if explicitly requested or nice-to-have.**

⚠️ **Warning:** Test brief does NOT require persistence. This adds complexity.

### SQLite Strategy
* **File-based** for local dev: `flowingly.db`
* **In-memory** for tests: `:memory:`
* **No external dependencies** (no Docker, no Postgres)

### Schema (Minimal)
```sql
CREATE TABLE messages (
    id TEXT PRIMARY KEY,
    correlation_id TEXT NOT NULL UNIQUE,
    content_hash TEXT NOT NULL,
    classification TEXT NOT NULL, -- 'expense' or 'other'
    created_at TEXT NOT NULL,
    UNIQUE(content_hash) -- Idempotency
);

CREATE TABLE expenses (
    id TEXT PRIMARY KEY,
    message_id TEXT NOT NULL REFERENCES messages(id),
    vendor TEXT,
    total REAL NOT NULL,
    total_excl_tax REAL NOT NULL,
    sales_tax REAL NOT NULL,
    cost_centre TEXT NOT NULL
);

CREATE TABLE other_payloads (
    id TEXT PRIMARY KEY,
    message_id TEXT NOT NULL REFERENCES messages(id),
    raw_tags TEXT NOT NULL -- JSON
);
```

### EF Core Setup
* **Microsoft.EntityFrameworkCore.Sqlite**
* **DbContext** with entities
* **Repository pattern**: `IMessageRepository`
* **Migration**: Single migration for schema

### Idempotency
* Hash input text (SHA-256)
* Check `content_hash` before processing
* If exists → return cached result (same correlation_id)

### Health Check
* **Endpoint**: `GET /api/v1/health`
* **Response**: `{ "status": "healthy", "database": "connected", "version": "1.0.0" }`

### DoD
* [ ] SQLite database created on first run
* [ ] Messages stored with correlation_id
* [ ] Idempotency works (same input → same correlation_id)
* [ ] Repository tests green (in-memory SQLite)
* [ ] Health check endpoint works
* [ ] Zero setup required (SQLite auto-created)

**Time estimate: 4 hours**

---

# Phase 3: Production Ready (Stretch Goals)

**Only if deploying to Render or demonstrating production skills.**

---

## M5 — Observability & Deployment (½ day)

**NOT required for test brief submission.**

### Structured Logging (Serilog)
* Correlation ID in every log line
* Log levels: Debug (dev), Information (prod)
* JSON format for structured logs

### Security Hardening
* **XML parser**: `DtdProcessing=Prohibit`, `XmlResolver=null`
* **Input limits**: 256KB max payload
* **API key middleware** (optional): Dev disabled, prod required

### Postgres Migration (if deploying)
* Render Postgres provisioning
* EF migrations compatibility (SQLite → Postgres)
* Connection string configuration

### Deployment (Render)
* `render.yaml` configuration
* CI/CD via GitHub Actions
* Health check endpoint for monitoring

### DoD
* [ ] Correlation IDs in all logs
* [ ] Secure XML parser configured
* [ ] Postgres migration tested
* [ ] Deployed to Render (if required)

**Time estimate: 4 hours**

---

# Test Coverage Summary

## Unit Tests (Parser Domain)
**Target: 30+ tests**

* Tag validation: 8 tests
* Required fields: 3 tests
* Normalization: 4 tests
* Banker's Rounding: 6 tests
* GST calculation: 4 tests
* Tax rate precedence: 2 tests
* Time parsing: 5 tests
* Sample fixtures: 2 tests

## Contract Tests (API)
**Target: 10+ tests**

* Happy paths: 2 tests (expense, other)
* Error scenarios: 4 tests (missing total, overlapping tags, invalid tax rate, invalid format)
* Contract enforcement: 2 tests (correlation ID, XOR expense/other)
* Tax rate: 2 tests (precedence, default)

## E2E Tests (UI)
**Target: 5+ tests**

* Happy path expense: 1 test
* Happy path other: 1 test
* Error display: 2 tests (missing total, overlapping tags)
* GST calculation: 1 test (Banker's Rounding verification)

**Total: 45+ tests**

---

# Acceptance Checklist

## Phase 1 (Minimum Submission)
* [ ] Clone repo and run in <5 minutes (no Docker/DB setup)
* [ ] `dotnet run` starts API on port 5001
* [ ] `npm run dev` starts UI on port 5173
* [ ] Paste sample email from test brief → correct JSON with GST
* [ ] Missing `<total>` → 400 error with `MISSING_TOTAL` code
* [ ] Overlapping tags → 400 error with `UNCLOSED_TAGS` code
* [ ] Missing `<cost_centre>` → defaults to `"UNKNOWN"` (no error)
* [ ] GST calculation correct (Banker's Rounding verified)
* [ ] Unit tests: 30+ tests, all green
* [ ] Contract tests: 10+ tests, all green
* [ ] E2E tests: 5+ tests, all green
* [ ] README has clear quick start (<2 min to run)
* [ ] Sample fixtures from test brief included

## Phase 2 (Polish)
* [ ] Swagger UI at `/swagger` with examples
* [ ] Docker compose works (optional)
* [ ] Enhanced README with troubleshooting
* [ ] Makefile commands work
* [ ] Persistence with SQLite (optional)

## Phase 3 (Production)
* [ ] Structured logging with correlation IDs
* [ ] Security hardening (XML parser, input limits)
* [ ] Deployed to Render (optional)

---

# Time Estimates

| Phase | Milestones | Time | Status |
|-------|------------|------|--------|
| **Phase 1** | M0 + M1 + M2 + M3 | 2-3 days | ✅ SUBMITTABLE |
| **Phase 2** | M6 + M4 (optional) | 1 day | Polish |
| **Phase 3** | M5 | ½ day | Stretch |

**Recommended:** Complete Phase 1, submit, then add Phase 2 polish if time permits.

---

# Engineering Decisions (Quick Reference)

| Decision | Value | Why |
|----------|-------|-----|
| **Rounding** | Banker's Rounding (ToEven) | Financial standard, unbiased |
| **Tag Validation** | Stack-based parser | Detects overlaps, not just unclosed |
| **Tax Rate** | Request > Config (0.15) | Flexibility with safe default |
| **Database** | None (Phase 1), SQLite (Phase 2) | Zero setup friction |
| **Deployment** | Local only (Phase 1), Render (Phase 3) | Focus on correctness first |
| **UI** | Minimal React (Phase 1), Polish (Phase 2) | Functional over fancy |
| **Docker** | Optional (Phase 2) | Simpler setup without |

---

# What Makes This Optimized

## Compared to Original Plan

| Aspect | Original Plan | Optimized Plan | Benefit |
|--------|---------------|----------------|---------|
| **Submittable at** | M6 (5 days) | M3 (2.5 days) | 2x faster to submission |
| **Docker required** | M0 (blocker) | M6 (optional) | Easier reviewer setup |
| **Persistence** | M4 (required) | M4 (optional) | Reduces complexity |
| **UI complexity** | Full-featured | Minimal → polish | Faster implementation |
| **Sample fixtures** | M6 | M1 | Earlier validation |
| **Focus** | Complete all milestones | Submittable first, polish later | Risk mitigation |

## Key Optimizations

1. **Zero external dependencies in Phase 1** — No Docker, no DB, just .NET + Node
2. **Sample fixtures early** — Test against brief from day 1
3. **Minimal UI first** — Functional beats fancy for speed
4. **Persistence optional** — Not evaluated, adds risk
5. **Swagger in Phase 2** — Nice-to-have, not critical for submission
6. **Clear submission gate** — Know when you're done vs. when you're polishing

---

# Success Metrics

**You know you're ready to submit when:**

✅ Reviewer can clone and run in <5 minutes
✅ All sample emails from test brief work correctly
✅ All tests green (45+ tests)
✅ GST calculation verified (Banker's Rounding)
✅ Tag validation catches overlaps
✅ Error codes clear and specific
✅ UI functional (not fancy, but works)
✅ README has 3-step quick start

**Everything else is bonus points.**

---

**Last Updated:** 2025-10-06
**Optimization Focus:** Fastest path to submittable product, then polish
**Target:** Phase 1 complete in 2-3 days, submit, add Phase 2 if time permits

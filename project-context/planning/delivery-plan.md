
# Goal (single sentence)

Based on project-context/specifications/prd-technical_spec.md (Flowingly Parsing Service — PRD + Technical Specification (v0.2)), deliver a small, correct, well-tested **.NET Web API + React UI** that parses the provided “receipt-like” text, validates tags, computes **GST-exclusive total** from an **inclusive** `<total>`, and surfaces clear errors — with a crisp README and one-click run.

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

  * Reject **unbalanced or overlapping** tags (e.g., `<a><b></a></b>`). Error: `UNCLOSED_TAGS`.
  * If `<total>` **missing**: `MISSING_TOTAL`.
  * If `<cost_centre>` missing: default to `"UNKNOWN"`.
  * Prefer `<expense>`-scoped `<total>` over global if both exist.
  * Number normalization: strip currency symbols/commas; use **decimal(18,2)** for money; **round half away from zero** at presentation boundaries (document this).
* **Parser**: Pure function(s) with unit tests (happy + edge + failure).
* **DoD**

  * Unit tests for: missing `<total>`, default cost centre, overlapping tags, currency/commas, precedence.

## M2 — Domain + API contract (½–1 day)

**Purpose:** Wrap parsing in the API surface exactly as spec’d.

* **Contract**

  * Request: `{ text: string, taxRate?: number }` (if absent → default 0.15).
  * Response (success): `{ classification: "expense"|"other", expense?: {...}, other?: {...}, correlation_id }`
  * Response (error): `{ error: { code, message, details? }, correlation_id }` with `400`.
* **Tax math**

  * Inputs are **tax-inclusive** totals. Compute:

    * `total_excl_tax = total_incl / (1 + rate)`
    * `tax = total_incl - total_excl_tax`
    * Round result to 2dp (policy above).
* **Swagger**: Add **concrete examples** (happy path + each error).
* **DoD**

  * Contract tests (WebApplicationFactory) verify codes + payload shape
  * Swagger examples visible and copy-pastable

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

## M4 — Persistence (optional, ½ day if kept)

**Purpose:** Keep a tiny audit trail of requests/responses (if you still want it).

* **Decision**: Either **Postgres everywhere** (Testcontainers for tests) or **SQLite only for tests** + Postgres for dev/prod. Pick one and **state it** in the README.
* Store: id, timestamp, hash(text), classification, totals, error_code/null.
* Add `/api/v1/health` to check DB + version.

## M5 — Observability & hardening (½ day)

**Purpose:** Production hygiene without scope creep.

* **Serilog**: structured logs, include `correlation_id` on each log line.
* **Rate-limit**: simple in-memory token bucket per API key/IP (if time; otherwise backlog).
* **API key**: tiny middleware that checks `x-api-key` (dev default enabled, prod required).
* **Error map**: centralized problem details.

## M6 — Packaging & demo (½ day)

**Purpose:** Make it **reviewer-friendly**.

* **README (2-minute path)**

  * Prereqs (Docker/Node/.NET).
  * One command: `docker compose up`
  * Open UI URL and paste the **sample** input; show expected JSON.
  * Swagger URL; “Try it out” instructions.
  * Troubleshooting tips (ports, CORS, API key).
* **Makefile/NPM scripts**: `make dev`, `make test`, `npm run e2e`.

---

# Test plan (what we actually verify)

**Unit (parser):**

* ✅ Balanced vs overlapping/unclosed tags → `UNCLOSED_TAGS`
* ✅ Missing `<total>` → `MISSING_TOTAL`
* ✅ Missing `<cost_centre>` → `"UNKNOWN"`
* ✅ Currency + commas: `<total>$35,000.00</total>` → `35000.00`
* ✅ Two totals (global vs expense) with different values → **expense wins**
* ✅ Rounding half cases: `100.005` @ 2dp → `100.01` (half-away-from-zero)

**Contract/integration:**

* `400` codes map to correct `{ error: { code, message } }` payload
* `taxRate` precedence (request > config default (0.15))
* Correlation id always present

**E2E (Playwright):**

* Happy path: paste sample, click “Parse”, shows computed values
* Missing total: UI surfaces error.text
* Unbalanced tags: UI surfaces error.text

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

* **Money & rounding**: Decimal math, **half away from zero**, 2dp output.
* **Tag validator**: treat **overlaps** as invalid (not just unclosed).
* **Tax source of truth**: Request `taxRate` > config default (0.15).
* **DB stance**: Either **Postgres everywhere** (with Testcontainers) *or* “SQLite for tests only, Postgres for dev/prod.” Document it decisively.
* **Security**:

  * XML parser hardened (no DTD, no external entities, size/time caps)
  * API key middleware (dev default, prod required)
* **Non-goals (v1)**: No fuzzy NER, no multi-currency, no time parsing beyond a whitelisted format; no reservation logic beyond “other”.

---

# What to implement first (sequence inside each milestone)

1. **M0:** Start from the **README** stub + docker compose; wire echo → UI.
2. **M1:** Finish **parser** as pure lib with unit tests.
3. **M2:** Wrap in API handlers; lock error contract; add Swagger **examples**.
4. **M3:** UI + Playwright.
5. **(Optional) M4:** Persistence with a single table and repository.
6. **M5:** Serilog, correlation id everywhere; (optional) rate limit.
7. **M6:** README polish + demo script.

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


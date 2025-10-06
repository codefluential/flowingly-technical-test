# Flowingly Parsing Service — Repository Review

## Overview
- **Scope**: modular ingestion pipeline that parses tagged free-form text (initially expense claims), normalizes NZ GST totals, and returns structured JSON.
- **Current milestone**: M1 core parsing/validation completed; M2 API contract work queued. Tracking system shows 30/50 tasks done (M0+M1) with 18 unit tests in place.
- **Architecture**: Clean/Hexagonal .NET 8 backend (Api → Application → Domain ← Infrastructure) with a React + Vite(TypeScript) client; rich documentation/ADR ecosystem under `project-context/`.

## Backend Notes
- **API surface**: single `POST /api/v1/parse` endpoint (`src/Api/Endpoints/ParseEndpoint.cs`) currently returns a mocked expense payload + correlation metadata (M0 echo). Swagger wired for dev.
- **Domain pipeline components** (already implemented):
  - Secure XML island extractor (`XmlIslandExtractor`) rejects DTD/XXE/DoS inputs and validates `<expense>…</expense>` fragments.
  - Stack-based tag validator (`TagValidator`) enforces proper nesting, emitting `UNCLOSED_TAGS` errors via `ValidationException`.
  - NZ GST calculator (`TaxCalculator`) uses Banker's rounding through `IRoundingHelper` for financial precision.
  - `ExpenseProcessor` runs a five-stage flow (Validate → Extract → Normalize → Persist → BuildResponse) and merges inline tags with XML islands; uses `ITaxCalculator` + `IExpenseRepository` abstractions.
  - `ContentRouter` selects the first processor able to handle `ParsedContent`, falling back to a yet-to-be-added `other` processor.
- **Gaps**: Application and Infrastructure projects are largely placeholders; repository implementations, DI wiring, and persistence are not present. API response contract currently diverges from domain/TS DTO expectations (mock payload vs `ParseResponse`/`ExpenseData`).

## Frontend Notes
- React app (`client/src/App.tsx`) offers textarea input, calls `parseText` (fetch-based client), and renders `ResultDisplay` for expense vs other classifications with correlation ID footer.
- TypeScript contracts (`client/src/types/api.ts`) model a discriminated union (`ExpenseResponse | OtherResponse`) including `meta` block and richer `expense` details than the backend currently returns.
- Styling/layout delivered via CSS modules in `client/src/App.css` and component-level markup (e.g., `ParseForm`, `ErrorBanner`).

## Testing & Quality Systems
- 18 unit tests across domain helpers/processors emphasize security (XXE, DTD, DoS), correctness (GST rounding edge cases), and pipeline behavior (`tests/Flowingly.ParsingService.UnitTests`).
- `TEST_STRATEGY.md` details the RED→GREEN workflow, naming conventions, and coverage targets (30+ unit tests planned for M1).
- Fixtures under `fixtures/` provide representative expense/reservation emails for manual/E2E scenarios.

## Risks / Follow-Ups
1. Align backend `ParseResponse` serialization with shared contracts and frontend TypeScript expectations before M2 delivery.
2. Implement missing `OtherProcessor`, repository adapters, and DI registration to complete the Clean Architecture loop.
3. Expand test suite toward the documented 30-test goal as new functionality lands (especially parser integration and API-level behaviors).
4. Ensure API mock response is replaced with the real parsing pipeline once infrastructure pieces are in place.

## References
- `CLAUDE.md` — project charter, milestones, environment setup, and ADR pointers.
- `README.md` — quick start + architectural overview.
- Domain implementations under `src/Domain` and unit tests under `tests/Flowingly.ParsingService.UnitTests`.

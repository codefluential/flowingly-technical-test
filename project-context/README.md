# Project Context Documentation

This directory contains comprehensive documentation for the **Flowingly Parsing Service** project.

## 📁 Documentation Structure

### 📋 Specifications (`/specifications`)
- **[prd-technical_spec.md](specifications/prd-technical_spec.md)** — PRD v0.3 (current)
  - Complete product requirements and technical design
  - API contracts, validation rules, error handling

### 🏗️ Architecture Decision Records (`/adr`)
**10 ADRs documenting key architectural decisions:**
- ADR-0001: Storage choice (SQLite dev, Postgres production)
- ADR-0002: Clean Architecture + CQRS-lite
- ADR-0003: Processor Strategy pattern
- ADR-0004: Swagger API documentation
- ADR-0005: URI-based versioning
- ADR-0006: API key authentication
- ADR-0007: Response contract design (XOR structure)
- ADR-0008: Parsing & validation rules
- ADR-0009: Banker's Rounding policy
- ADR-0010: Test strategy & coverage

See **[adr/README.md](adr/README.md)** for complete index.

### 📊 Requirements & Analysis (`/requirements-and-analysis`)
- **Full Stack Engineer Test (Sen) V2.pdf** — Original test brief
- Requirements analysis and compliance mapping

### 📝 Implementation (`/implementation`)
- **PROGRESS.md** — Final project status (51/51 tasks, 195 tests)
- **SMOKE_TEST_REPORT.md** — Production readiness validation

### 📅 Planning (`/planning`)
- **delivery-plan.md** — Milestone-based delivery roadmap (M0–M3)

---

## 🎯 Quick Navigation

**Starting Review:**
1. Read [prd-technical_spec.md](specifications/prd-technical_spec.md) (requirements)
2. Review [adr/README.md](adr/README.md) (architectural decisions)
3. Check [implementation/PROGRESS.md](implementation/PROGRESS.md) (completion status)

**Understanding Architecture:**
- ADR-0002: Architecture pattern
- ADR-0003: Content routing strategy
- ADR-0007: API response design

**Test Coverage:**
- ADR-0010: Test strategy (195 tests: 118 backend + 77 E2E)
- implementation/SMOKE_TEST_REPORT.md

---

## 🔑 Key Engineering Decisions

| Decision | Value | Reference |
|----------|-------|-----------| 
| **Rounding Policy** | Banker's Rounding (MidpointRounding.ToEven) | ADR-0009 |
| **Tag Validation** | Stack-based nesting validation | ADR-0008 |
| **Response Contract** | Classification-specific (expense XOR other) | ADR-0007 |
| **Database** | SQLite local/test, Postgres production | ADR-0001 |
| **Architecture** | Clean/Hexagonal + CQRS-lite | ADR-0002 |
| **API Versioning** | URI-based (`/api/v1/`) | ADR-0005 |

---

**Project Status:** ✅ Phase 1 Complete (M0–M3), Production Ready
**Test Coverage:** 195 passing tests (433% of 45+ target)
**Last Updated:** 2025-10-07

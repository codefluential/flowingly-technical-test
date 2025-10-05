# Architecture Decision Records (ADRs)

This directory contains Architecture Decision Records (ADRs) documenting significant architectural and technical decisions made during the development of the Flowingly Parsing Service.

## What are ADRs?

ADRs are lightweight documents that capture important architectural decisions along with their context and consequences. They help:

- **Preserve Context**: Explain why decisions were made, not just what was decided
- **Onboard Team Members**: Help new developers understand the reasoning behind the architecture
- **Avoid Revisiting Decisions**: Prevent rehashing settled debates
- **Enable Evolution**: Provide foundation for future architectural changes

## ADR Format

Each ADR follows this structure:

```markdown
# ADR-XXXX: [Title]

**Status**: [Proposed | Accepted | Deprecated | Superseded]
**Date**: YYYY-MM-DD
**Deciders**: [Names/Roles]

## Context

What is the issue we're facing? What factors are relevant?

## Decision

What architectural decision did we make?

## Consequences

What are the positive and negative outcomes of this decision?

## Alternatives Considered

What other options did we evaluate and why were they not chosen?
```

## Documented ADRs

The following ADRs have been documented:

### Storage and Architecture
- **ADR-0001**: Storage choice (Postgres for all environments) — **Superseded by ADR-0001-v2**
- **ADR-0001-v2**: Storage choice (SQLite local, Postgres deployment) — **Accepted**
- **ADR-0002**: Architecture style (Clean/Hexagonal + CQRS-lite) — **Accepted**

### API Design and Patterns
- **ADR-0003**: Processor Strategy pattern — **Accepted**
- **ADR-0004**: Swagger for API contract & onboarding — **Accepted**
- **ADR-0005**: Versioning via URI — **Accepted**
- **ADR-0006**: API key authentication — **Accepted**
- **ADR-0007**: Response Contract Design — **Accepted**

### Parsing and Validation
- **ADR-0008**: Parsing and Validation Rules — **Accepted**
- **ADR-0009**: Tax Calculation with Banker's Rounding — **Accepted**

### Testing
- **ADR-0010**: Test Strategy and Coverage — **Accepted**

## Naming Convention

ADRs are numbered sequentially: `ADR-0001-short-title.md`, `ADR-0002-short-title.md`, etc.

## Resources

- [Architecture Decision Records (ADR) - Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
- [ADR GitHub Organization](https://adr.github.io/)

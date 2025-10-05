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

## Planned ADRs

According to the PRD + Technical Specification (v0.1), the following ADRs are planned:

- **ADR-0001**: Storage choice (Postgres for all environments)
- **ADR-0002**: Architecture style (Clean/Hexagonal + CQRS-lite)
- **ADR-0003**: Processor Strategy pattern
- **ADR-0004**: Swagger for API contract & onboarding
- **ADR-0005**: Versioning via URI

## Naming Convention

ADRs are numbered sequentially: `ADR-0001-short-title.md`, `ADR-0002-short-title.md`, etc.

## Resources

- [Architecture Decision Records (ADR) - Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
- [ADR GitHub Organization](https://adr.github.io/)

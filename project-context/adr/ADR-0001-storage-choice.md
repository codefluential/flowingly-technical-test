# ADR-0001: Storage Choice - Postgres for All Environments

**Status**: Accepted
**Date**: 2025-10-05
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service requires persistent storage for:
- Raw inbound messages with content and metadata
- Normalized expense outputs (structured data extraction results)
- Other/unprocessed payloads for future processing
- Processing logs for auditing and debugging

Key factors influencing the storage decision:
1. **Development constraints**: Time-limited delivery (Sunday evening to Monday)
2. **Deployment platform**: Using Render free tier for all infrastructure
3. **Data model complexity**: Relational schema with FK constraints, indexes, and JSONB fields
4. **Team familiarity**: Standard SQL databases well-understood
5. **Production requirements**: Need durable, ACID-compliant storage
6. **Cost**: Must use free tier options

The original PRD proposed **SQLite for dev** and **Postgres for prod**, but this creates environment parity issues and additional configuration complexity.

## Decision

**Use PostgreSQL for all environments (development and production).**

Specifically:
- **Local Development**: PostgreSQL via Docker container or local installation (self-managed)
- **Production**: Render Managed Postgres (free tier, versions 13-17 available)
- **No SQLite**: Remove SQLite from technology stack entirely

### Render Postgres Details

- **Managed Service**: Fully managed PostgreSQL with automated backups, high availability, and connection pooling
- **Versions**: PostgreSQL 13-17 available for new instances (choose based on feature needs)
- **Connection**: Connection URLs exposed via Render dashboard; inject as environment variable
- **Benefits**: Native Render integration, managed backups, easy connection rotation, zero ops overhead

## Consequences

### Positive

1. **Environment Parity**: Identical database engine in dev and prod eliminates "works on my machine" issues
2. **Simplified Configuration**: Single connection string pattern; no conditional logic for different DB providers
3. **Feature Consistency**: All Postgres-specific features (JSONB, array types, advanced indexing) available in both environments
4. **Migration Safety**: EF Core migrations developed in dev will work identically in prod
5. **Render Integration**: Render provides free Postgres instances, making deployment straightforward
6. **No EF Provider Switching**: Avoid complexity of maintaining both `Npgsql.EntityFrameworkCore.PostgreSQL` and `Microsoft.EntityFrameworkCore.Sqlite`
7. **Testing Reliability**: Integration tests run against same DB engine as production

### Negative

1. **Local Setup Overhead**: Developers must install/run Postgres locally (vs. SQLite's zero-config, file-based approach)
2. **Docker Dependency** (optional): Many devs will use Docker for Postgres, adding tooling dependency
3. **Resource Usage**: Postgres consumes more memory/CPU than SQLite on dev machines
4. **Render Free Tier Limits**:
   - 256 MB RAM
   - 1 GB storage
   - Shared CPU
   - These are acceptable for demo/MVP scope but must be documented

### Mitigation Strategies

- Provide `docker-compose.yml` with Postgres service pre-configured for instant local setup
- Document Postgres installation steps in README for developers without Docker
- Keep database size minimal during development (seed data should be lightweight)
- Monitor Render Postgres metrics to ensure free tier suffices for demo load

## Alternatives Considered

### 1. SQLite (dev) + Postgres (prod) [Original PRD Proposal]

**Pros**:
- Zero-config local development
- Fast, lightweight, file-based
- No additional local services

**Cons**:
- Environment parity issues (different SQL dialects, feature sets)
- EF migrations may behave differently between providers
- JSONB queries in Postgres won't work in SQLite
- Risk of production bugs not caught in dev
- Additional configuration complexity (appsettings.Development.json vs Production.json with different providers)

**Rejected because**: Environment parity is critical for reliability, especially in time-constrained delivery.

### 2. In-Memory Database (dev) + Postgres (prod)

**Pros**:
- Ultra-fast tests
- No persistence between runs (clean slate)

**Cons**:
- Even worse environment parity than SQLite
- Cannot test migrations, constraints, or Postgres-specific features locally
- Integration tests would be less reliable

**Rejected because**: Not suitable for validating production behavior.

### 3. Neon Serverless Postgres (prod alternative)

**Pros**:
- Serverless, scales to zero
- More generous free tier (3 GB storage vs Render's 1 GB)
- Excellent performance

**Cons**:
- Requires separate platform account (Render handles all infra in this project)
- Adds deployment complexity (Render for API + Neon for DB)

**Rejected because**: Render's unified platform simplifies deployment; 1 GB is sufficient for MVP scope.

### 4. MySQL / MariaDB

**Pros**:
- Widely known, good free hosting options
- Mature ecosystem

**Cons**:
- Less advanced JSON support than Postgres JSONB
- EF Core support for Postgres is more robust
- Team preference for Postgres

**Rejected because**: Postgres JSONB and array features are valuable for this use case.

## Implementation Notes

### Connection Strings

- **Local Dev**: `Host=localhost;Database=flowingly_dev;Username=postgres;Password=dev`
- **Render Prod**: Provided via `DATABASE_URL` environment variable (format: `postgresql://user:pass@host:port/db`)

### Schema Management

- **Migrations**: Use EF Core migrations: `dotnet ef migrations add InitialCreate`
- **Foreign Keys**: All relationships enforced with `REFERENCES ... ON DELETE CASCADE` constraints
- **Indexes**: Create indexes on:
  - `messages.content_hash` (unique index for idempotency)
  - `expenses.message_id`
  - `processing_logs(message_id, created_at)`
- **ERD**: Document entity relationships in README using Mermaid or dbdiagram

### Idempotency Strategy

**Problem**: Duplicate processing of the same text content (e.g., retries, accidental resubmissions)

**Solution**: Content hash-based deduplication
- Compute SHA-256 hash of `content` field before insert
- Store hash in `messages.content_hash` column
- Create **unique index** on `content_hash`: `CREATE UNIQUE INDEX idx_messages_content_hash ON messages(content_hash);`
- On duplicate insert attempt, Postgres returns unique constraint violation
- Application catches exception and returns existing message ID (idempotent response)

**Benefits**:
- Safe retries (same input → same output, no duplicate records)
- Prevents accidental duplicate expense entries
- Simple implementation (database-enforced, no distributed locks needed)

### Local Docker Setup

```bash
# Run Postgres 16 in Docker
docker run -d \
  --name flowingly-postgres \
  -p 5432:5432 \
  -e POSTGRES_PASSWORD=dev \
  -e POSTGRES_DB=flowingly_dev \
  postgres:16-alpine

# Apply migrations
dotnet ef database update
```

## References

- PRD + Technical Specification (v0.2), Section 8: Data Model & Persistence
- PRD Review Notes: Question 6 confirmed "Just Postgres only, no SQLite"
- Render Postgres Documentation: https://render.com/docs/databases
- Render Postgres Versions: https://render.com/docs/databases#postgresql-versions
- EF Core Postgres Provider: https://www.npgsql.org/efcore/
- Idempotency Patterns: https://aws.amazon.com/builders-library/making-retries-safe-with-idempotent-APIs/

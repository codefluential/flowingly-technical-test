# ADR-0001-v2: Storage Choice - SQLite Local, Postgres Deployment

**Status**: Accepted
**Date**: 2025-10-06
**Supersedes**: ADR-0001
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service requires persistent storage for:
- Raw inbound messages with content and metadata
- Normalized expense outputs (structured data extraction results)
- Other/unprocessed payloads for future processing
- Processing logs for auditing and debugging

Key factors influencing the storage decision:
1. **Development constraints**: Time-limited delivery with emphasis on iteration speed
2. **Deployment platform**: Using Render free tier for production infrastructure
3. **Data model complexity**: Relational schema with FK constraints, indexes, and JSON fields
4. **Team familiarity**: Standard SQL databases well-understood
5. **Production requirements**: Need durable, ACID-compliant storage
6. **Testing requirements**: Fast, hermetic tests without external dependencies
7. **Cost**: Must use free tier options

**Change from ADR-0001**: The original decision mandated "Postgres for all environments" to ensure environment parity. However, external review feedback (PRD v0.3) highlighted that **iteration speed** and **fast test execution** are more critical for the MVP delivery timeline than absolute environment parity. SQLite for local development and tests significantly reduces friction while Postgres for deployment stages (M5+) ensures production readiness.

## Decision

**Use SQLite for local development and tests, PostgreSQL for deployment stages (M5+).**

Specifically:
- **Local Development**: SQLite (file-based or in-memory for tests)
- **Production (M5+ deployment)**: Render Managed Postgres (free tier)
- **Migration Path**: EF Core migrations validated against both providers during development

### SQLite for Local Development

- **File-based storage**: `Data Source=flowingly.db` for persistent local data
- **In-memory for tests**: `Data Source=:memory:` for fast, isolated unit/integration tests
- **Zero external dependencies**: No Docker, no services, instant setup
- **Fast test execution**: In-memory DB allows hermetic test isolation

### Postgres for Deployment

- **Managed Service**: Render Managed PostgreSQL (versions 13-17 available)
- **Connection**: Connection URLs exposed via Render dashboard; inject as environment variable
- **Benefits**: Native Render integration, managed backups, production-grade reliability

## Consequences

### Positive

1. **Fast Iteration**: SQLite's zero-config setup removes local development friction
2. **Hermetic Tests**: In-memory SQLite enables fast, isolated tests without cleanup overhead
3. **No External Dependencies**: Developers don't need Docker or local Postgres installation
4. **Resource Efficiency**: Minimal memory/CPU usage on dev machines
5. **Render Integration**: Render provides free Postgres instances for production deployment
6. **Validated Migration Path**: EF Core migrations tested against both providers before deployment

### Negative

1. **Environment Differences**: SQLite and Postgres have different feature sets and SQL dialects
2. **Feature Limitations**: Postgres-specific features (JSONB operators, advanced indexing) not available in SQLite
3. **Migration Validation Overhead**: Must validate EF migrations work on both SQLite and Postgres
4. **Production Risk**: Bugs related to Postgres-specific behavior may not be caught in local testing
5. **Configuration Complexity**: Requires conditional provider registration based on environment

### Mitigation Strategies

- **Abstract Postgres-specific features**: Avoid JSONB queries; use JSON serialization instead
- **Migration validation**: CI pipeline runs migrations against both SQLite and Postgres
- **Feature parity testing**: Integration tests run against both providers in CI
- **Keep schema simple**: Use standard SQL features supported by both databases
- **Document differences**: Maintain list of known behavioral differences in README

## SQLite Considerations

### File-Based Mode (Development)

```
Data Source=flowingly.db;Cache=Shared;Mode=ReadWriteCreate
```

**Use Cases**:
- Local development with persistent data between runs
- Manual testing with seeded data
- Debugging with inspectable database file

**Benefits**:
- Persistent storage for inspection
- Can use DB browser tools (DB Browser for SQLite)
- Survives application restarts

### In-Memory Mode (Testing)

```
Data Source=:memory:
```

**Use Cases**:
- Unit tests requiring database interaction
- Integration tests with fast setup/teardown
- Hermetic test isolation

**Benefits**:
- Extremely fast test execution (no I/O)
- Perfect isolation (each test gets fresh DB)
- No cleanup required (DB destroyed when connection closes)

### Feature Limitations

**SQLite does NOT support**:
- JSONB operators (use JSON serialization instead)
- Array types (use delimited strings or separate tables)
- Advanced indexing (partial indexes, expression indexes)
- Concurrent writes (single-writer model)

**Workarounds**:
- Store JSON as TEXT, deserialize in application layer
- Use EF Core's value converters for complex types
- Keep concurrent write load minimal (single-user MVP scope)

## Postgres Migration Path

### Development Stage (M0-M4)

- Use SQLite exclusively for local dev and tests
- Validate EF migrations work on SQLite
- Abstract any Postgres-specific features behind repository interfaces

### Deployment Stage (M5+)

- Provision Render Postgres instance
- Run EF migrations against Postgres: `dotnet ef database update`
- Validate migration compatibility:
  - Check for schema drift between SQLite and Postgres
  - Test JSONB/JSON serialization compatibility
  - Verify constraint behavior matches expectations
- Deploy API with Postgres connection string from Render

### Schema Compatibility Testing

**Strategy**: CI pipeline validates migrations on both providers

```bash
# SQLite migration validation
dotnet ef database update --connection "Data Source=:memory:"

# Postgres migration validation (CI only, using test instance)
dotnet ef database update --connection "Host=test-db;Database=ci_test;..."
```

**Test Coverage**:
- Foreign key constraints behave identically
- Unique indexes work the same way
- JSON serialization/deserialization works across both
- Date/time handling is consistent

## Alternatives Considered

### 1. Postgres for All Environments [ADR-0001 Original Decision]

**Pros**:
- Perfect environment parity
- Postgres-specific features available everywhere
- No migration validation overhead

**Cons**:
- Requires Docker or local Postgres installation (setup friction)
- Slower test execution (external DB process)
- Higher resource usage on dev machines
- Not hermetic (tests share DB state unless carefully isolated)

**Rejected because**: Iteration speed and test performance are more critical for MVP timeline than absolute parity.

### 2. In-Memory Database (Microsoft.EntityFrameworkCore.InMemory)

**Pros**:
- Ultra-fast tests
- No persistence between runs

**Cons**:
- Not a real database (doesn't validate SQL, constraints, or migrations)
- Cannot test EF migrations
- Poor preparation for production behavior

**Rejected because**: EF In-Memory provider is too different from real SQL databases.

### 3. Always Use Postgres (Even for Tests)

**Pros**:
- True production parity

**Cons**:
- Tests require running Postgres instance (Docker or service)
- Slower test execution
- Complex test isolation (must clean DB between tests)

**Rejected because**: Test speed is critical for TDD workflow during time-constrained delivery.

## Implementation Notes

### Connection Strings

**Local Dev (SQLite)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=flowingly.db;Cache=Shared"
  }
}
```

**Tests (In-Memory SQLite)**:
```csharp
var connection = new SqliteConnection("Data Source=:memory:");
connection.Open(); // Keep connection open to persist in-memory DB
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connection)
    .Options;
```

**Render Prod (Postgres)**:
```bash
DATABASE_URL=postgresql://user:pass@host:port/db
```

### EF Core Provider Configuration

```csharp
// Program.cs or Startup.cs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var isProduction = builder.Environment.IsProduction();

if (isProduction)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
}
```

### Schema Management

- **Migrations**: Use EF Core migrations: `dotnet ef migrations add InitialCreate`
- **Foreign Keys**: All relationships enforced with `REFERENCES ... ON DELETE CASCADE` constraints
- **Indexes**: Create indexes on:
  - `messages.content_hash` (unique index for idempotency)
  - `expenses.message_id`
  - `processing_logs(message_id, created_at)`
- **JSON Storage**: Use TEXT columns with JSON serialization (compatible with both SQLite and Postgres)

### Idempotency Strategy

**Problem**: Duplicate processing of the same text content (e.g., retries, accidental resubmissions)

**Solution**: Content hash-based deduplication
- Compute SHA-256 hash of `content` field before insert
- Store hash in `messages.content_hash` column
- Create **unique index** on `content_hash`
- On duplicate insert attempt, database returns unique constraint violation
- Application catches exception and returns existing message ID (idempotent response)

**Benefits**:
- Safe retries (same input → same output, no duplicate records)
- Prevents accidental duplicate expense entries
- Works identically in SQLite and Postgres

### Local Development Setup

**Option 1: File-Based SQLite (Recommended for Development)**
```bash
# No setup required! Just run:
dotnet run

# Database file created automatically: flowingly.db
```

**Option 2: In-Memory SQLite (Tests Only)**
```bash
dotnet test  # Tests use in-memory DB automatically
```

**Option 3: Postgres (Optional, for Production Parity Validation)**
```bash
docker run -d \
  --name flowingly-postgres \
  -p 5432:5432 \
  -e POSTGRES_PASSWORD=dev \
  -e POSTGRES_DB=flowingly_dev \
  postgres:16-alpine

# Update appsettings.Development.json with Postgres connection string
dotnet ef database update
```

## References

- PRD + Technical Specification v0.3, Section 8: Data Model & Persistence
- PRD v0.3, Section 16: Migration Path (SQLite → Postgres)
- PRD v0.3, Section 17: Database Schema
- PRD v0.3, Section 14: ADRs & Implementation Logs
- EF Core SQLite Provider: https://learn.microsoft.com/en-us/ef/core/providers/sqlite/
- EF Core Postgres Provider: https://www.npgsql.org/efcore/
- Render Postgres Documentation: https://render.com/docs/databases

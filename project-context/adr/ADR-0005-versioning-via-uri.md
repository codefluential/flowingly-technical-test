# ADR-0005: API Versioning via URI Path

**Status**: Accepted
**Date**: 2025-10-05
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service API will evolve over time:
- New fields added to request/response DTOs
- Validation rules change
- New endpoints introduced
- Breaking changes to existing contracts

When breaking changes occur, we must support existing API consumers while offering new functionality. The system needs a versioning strategy that:

1. **Prevents breaking existing clients**: Old clients (e.g., React app v1.0) continue working when API v2 is released
2. **Enables parallel versions**: Both `/api/v1/parse` and `/api/v2/parse` can coexist
3. **Clear and explicit**: Clients explicitly choose which version to use
4. **Simple to implement**: Minimal infrastructure for Phase 1
5. **Discoverable**: Developers can easily see which version they're using (in URLs, logs, Swagger)

The PRD specifies "Versioning via URI" as the chosen approach, with a note to consider media-type header versioning as an alternative.

**Versioning Scope**: Phase 1 starts with **v1**. This ADR establishes the pattern for future v2, v3, etc.

## Decision

**Use URI path-based versioning with `/api/v{N}/` prefix.**

### Versioning Format

- **Pattern**: `/api/v1/`, `/api/v2/`, etc.
- **Example Endpoints**:
  - `POST /api/v1/parse` (Phase 1)
  - `POST /api/v2/parse` (future)
  - `GET /api/v2/expenses/{id}` (future)

### Version Indicator in Responses

Include `x-api-version` header in all responses:
```http
HTTP/1.1 200 OK
x-api-version: v1
Content-Type: application/json
...
```

### Default Behavior

- **No default version**: Clients must explicitly specify `/api/v1/` (no `/api/parse` shortcut)
  - Forces intentional version selection
  - Prevents accidental breaking changes when default version changes
- **Requests to `/api/parse` (no version)**: Return 404 or redirect to `/api/v1/parse` with a warning header

### Version Lifecycle

- **Active Support**: v1 and v2 both active when v2 launches
- **Deprecation**: When v3 launches, mark v1 as deprecated (return `Deprecation` header)
  ```http
  Deprecation: @1704067200 (Date: 2025-01-01 00:00:00 GMT)
  Link: </api/v3/parse>; rel="successor-version"
  ```
- **Sunset**: Announce sunset date; remove deprecated versions after grace period (e.g., 6 months)

## Consequences

### Positive

1. **Explicit and Visible**: Version is in the URL; developers immediately see which version they're using
   - Logs: `POST /api/v1/parse 200` vs `POST /api/v2/parse 200`
   - Easy to grep logs by version
   - No hidden version in headers or content negotiation

2. **Browser/Tool Friendly**: Works in browsers, curl, Postman without custom headers
   ```bash
   curl -X POST https://api.flowingly.com/api/v1/parse -d '{"text":"..."}'
   ```

3. **Routing Simplicity**: ASP.NET Core routing handles versions naturally:
   ```csharp
   app.MapPost("/api/v1/parse", HandleV1Parse);
   app.MapPost("/api/v2/parse", HandleV2Parse);
   ```

4. **Parallel Development**: v1 and v2 endpoints can have separate controllers/handlers/tests

5. **Swagger Integration**: Swagger UI can show v1 and v2 as separate documents:
   - `/swagger/v1/swagger.json`
   - `/swagger/v2/swagger.json`

6. **Caching Friendly**: CDN/proxy cache rules can differ by version (`/api/v1/*` vs `/api/v2/*`)

7. **Easy Migration**: Clients migrate incrementally (update URL from `/api/v1/` to `/api/v2/`)

8. **No Ambiguity**: No confusion about which version is used (unlike header-based versioning where header might be missing)

### Negative

1. **URL Pollution**: Versioning in URL means:
   - Breaking changes require new URL
   - Can't "hide" versioning from URL (some see this as inelegant)

2. **Resource Duplication**: Same resource (`/expenses/123`) exists at multiple URLs:
   - `/api/v1/expenses/123`
   - `/api/v2/expenses/123`
   - Not a true RESTful resource (same resource, different representations)

3. **Client Hardcoding**: Clients hardcode version in every API call:
   ```typescript
   const API_BASE = "https://api.flowingly.com/api/v1";
   fetch(`${API_BASE}/parse`, ...);
   ```
   - Requires code change to switch versions (but this is intentional)

4. **Routing Overhead**: More route definitions in code (one per version per endpoint)

5. **Versioning Granularity**: Can't version individual resources; entire API is versioned together
   - Example: Can't have `GET /api/v2/expenses/{id}` while keeping `POST /api/v1/parse`
   - Mitigation: Version namespaces if needed (`/api/expenses/v1/`, `/api/parsing/v1/`)

## Alternatives Considered

### 1. Media-Type Header Versioning (Content Negotiation)

Clients specify version via `Accept` header:
```http
GET /api/parse
Accept: application/vnd.flowingly.v1+json
```

**Pros**:
- Clean URLs (no version in path)
- RESTful (same resource URL, different representations)
- Granular (can version individual resources)

**Cons**:
- Hidden: Version not visible in URL; harder to debug
- Tooling: Requires custom headers in every tool (curl, Postman, browser)
- Caching: More complex (cache key includes `Accept` header)
- Default behavior: What if `Accept` header missing? Assume v1 or error?
- Routing complexity: ASP.NET Core routing doesn't naturally support header-based routing

**Rejected because**: Less explicit than URI versioning; harder to use and debug.

### 2. Query Parameter Versioning

Clients specify version via query param:
```http
GET /api/parse?version=1
```

**Pros**:
- Simple to implement
- Visible in URL

**Cons**:
- Looks hacky/unprofessional
- Query params typically reserved for filtering/search, not versioning
- Caching issues (query params often excluded from cache keys)
- Easy to forget (optional param)

**Rejected because**: Not a standard practice; URI path versioning is cleaner.

### 3. Subdomain Versioning

Different versions on different subdomains:
```
https://v1.api.flowingly.com/parse
https://v2.api.flowingly.com/parse
```

**Pros**:
- Complete isolation (separate deployments, databases, monitoring)
- Easy to route via DNS/load balancer

**Cons**:
- Infrastructure complexity (multiple subdomains, SSL certs)
- CORS issues (cross-origin requests between subdomains)
- Overkill for Phase 1

**Rejected because**: Too complex for monolith deployment on Render free tier.

### 4. No Versioning

Just make backward-compatible changes; never break API.

**Pros**:
- Simplest possible

**Cons**:
- Unrealistic; eventually breaking changes are needed (e.g., rename field, change validation)
- Accumulates technical debt (can't remove deprecated fields)

**Rejected because**: Not sustainable long-term.

## Implementation Guidelines

### Route Definition (Minimal API)

```csharp
// Program.cs
var v1 = app.MapGroup("/api/v1");

v1.MapPost("/parse", async (ParseRequestV1 request, IMediator mediator) =>
{
    var result = await mediator.Send(new ParseMessageCommand(request.Text, request.TaxRate, request.Currency));
    return result.Success ? Results.Ok(result.Data) : Results.BadRequest(result.Error);
})
.WithName("ParseMessage_V1");

// Future v2 example
var v2 = app.MapGroup("/api/v2");
v2.MapPost("/parse", async (ParseRequestV2 request, IMediator mediator) =>
{
    // v2 logic with new fields
})
.WithName("ParseMessage_V2");
```

### Response Header Middleware

```csharp
app.Use(async (context, next) =>
{
    await next();

    // Add version header to all responses
    if (context.Request.Path.StartsWithSegments("/api/v1"))
    {
        context.Response.Headers.Add("x-api-version", "v1");
    }
    else if (context.Request.Path.StartsWithSegments("/api/v2"))
    {
        context.Response.Headers.Add("x-api-version", "v2");
    }
});
```

### Swagger Configuration

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Flowingly API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "Flowingly API", Version = "v2" });
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flowingly API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Flowingly API v2");
});
```

### Client-Side (React)

```typescript
// src/config.ts
export const API_CONFIG = {
  baseUrl: import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001',
  version: 'v1',
};

export const getApiUrl = (endpoint: string) =>
  `${API_CONFIG.baseUrl}/api/${API_CONFIG.version}${endpoint}`;

// src/api/parse.ts
import { getApiUrl } from '../config';

export const parseText = async (text: string) => {
  const response = await fetch(getApiUrl('/parse'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text }),
  });
  return response.json();
};
```

### Version Deprecation (Future)

When deprecating v1:
```csharp
if (context.Request.Path.StartsWithSegments("/api/v1"))
{
    context.Response.Headers.Add("Deprecation", "@1704067200"); // Unix timestamp
    context.Response.Headers.Add("Link", "</api/v2/parse>; rel=\"successor-version\"");
}
```

## Breaking vs Non-Breaking Changes

**Non-Breaking** (can be added to existing version):
- Add optional request fields
- Add response fields
- Add new endpoints
- Relax validation (accept more input)

**Breaking** (require new version):
- Remove/rename request fields
- Remove/rename response fields
- Change field types (string → number)
- Tighten validation (reject previously valid input)
- Change endpoint URLs

## Version Transition Strategy

When releasing v2:
1. Deploy v2 alongside v1 (both active)
2. Update Swagger to show both versions
3. Add deprecation warning to v1 responses
4. Update React app to use v2 (configure `API_CONFIG.version = 'v2'`)
5. Monitor v1 usage; sunset after 6 months if zero traffic

## References

- PRD + Technical Specification (v0.1), Section 12: Versioning via URI
- PRD Review Notes: Question 7 confirmed "URI-only" versioning (not media-type)
- Microsoft API Versioning Guidance: https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/versioning
- REST API Versioning Strategies: https://www.baeldung.com/rest-versioning
- HTTP Deprecation Header: https://datatracker.ietf.org/doc/html/draft-ietf-httpapi-deprecation-header

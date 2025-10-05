# ADR-0006: API Key Authentication for Production

**Status**: Accepted
**Date**: 2025-10-05
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service exposes a public HTTP API (`POST /api/v1/parse`) that processes text content and performs computational work (parsing, validation, normalization, database writes). This raises several concerns:

1. **Abuse Prevention**: Without authentication, anyone on the internet can:
   - Submit unlimited requests (resource exhaustion)
   - Process malicious/spam content
   - Generate unnecessary database records and logs

2. **Rate Limiting Foundation**: To implement per-client rate limits, we need to identify clients

3. **Usage Tracking**: Understanding which clients use the API helps with:
   - Capacity planning
   - Debugging (correlate issues to specific clients)
   - Future billing/quotas (if service becomes commercial)

4. **Production Hygiene**: Demonstrates professional API design practices expected in production systems

5. **Development Experience**: Authentication should not hinder local development or testing

**Requirements from PRD v0.2**:
- Section 17, Question 3: "Auth: **Enabled**" with rationale: "Demonstrates production hygiene (prevents casual misuse; enables per-key rate limiting)"
- Section 7: Provides full middleware code example for API key checking

**Constraints**:
- Phase 1 MVP scope (no complex OAuth/JWT infrastructure)
- Free tier deployment (Render) - no paid auth services
- Simple implementation (days, not weeks)
- Backward compatibility with local development workflow

## Decision

**Implement header-based API key authentication for Production environment only.**

### Key Design Choices

1. **Header-Based**: Client sends API key via `X-API-Key` request header
2. **Environment-Specific**:
   - **Production**: API key **required**; 401 Unauthorized if missing/invalid
   - **Development**: API key **disabled**; all requests allowed (frictionless dev/test)
3. **Configuration-Driven**: API key stored in `appsettings.Production.json` or environment variable
4. **Single Shared Key**: One API key shared across all authorized clients (Phase 1 simplicity)
5. **Middleware Implementation**: Custom ASP.NET Core middleware checks header before routing

### API Key Lifecycle

- **Generation**: Generate strong random key (e.g., `openssl rand -base64 32`)
- **Storage**: Store in Render environment variable `Security__ApiKey`
- **Distribution**: Share key with authorized clients (React UI, QA team, reviewers)
- **Rotation**: Manual rotation via Render dashboard (update env var, redeploy)

## Consequences

### Positive

1. **Abuse Prevention**: Prevents casual misuse; attacker needs valid key to submit requests

2. **Rate Limiting Ready**: Can track requests per API key; future enhancement: multiple keys with different rate limits

3. **Usage Attribution**: Log API key hash (first 8 chars) to correlate requests with clients

4. **Zero Dev Friction**: Local development workflow unchanged (no key needed); developers don't manage secrets

5. **Simple Implementation**: ~20 lines of middleware code; no external dependencies

6. **Production-Ready**: Demonstrates understanding of API security best practices

7. **Auditable**: Can revoke/rotate key if compromised; logs show when key was used

8. **Cost-Free**: No third-party auth service fees (unlike Auth0, Firebase Auth)

### Negative

1. **Shared Key Weakness**: Single key means:
   - If leaked, all clients compromised (must rotate key and redistribute)
   - Cannot distinguish between clients (all requests look the same)
   - Cannot selectively revoke access to one client

2. **Manual Distribution**: Key must be shared with clients out-of-band (email, Slack, config)

3. **No Fine-Grained Permissions**: All authenticated clients have same access (cannot restrict endpoints per key)

4. **Rotation Downtime**: Rotating key requires redeploying app and updating all clients (brief service disruption)

5. **Header Visibility**: API key visible in request headers (less secure than signed tokens)
   - Mitigation: **HTTPS only** (header encrypted in transit)
   - Mitigation: Warn users not to log/cache API key in browser

6. **Not Standards-Based**: Custom header (`X-API-Key`) instead of standard `Authorization: Bearer <token>`

### Mitigation Strategies

- **HTTPS Enforcement**: Render serves over HTTPS by default; reject HTTP requests
- **Key Strength**: Use cryptographically secure random keys (32+ bytes, base64 encoded)
- **Logging**: Log API key hash (not full key) for debugging: `SHA256(key).Substring(0, 8)`
- **Rotation Plan**: Document key rotation procedure in README; plan quarterly rotations
- **Rate Limiting**: Implement IP-based rate limiting as additional layer (future ADR)

## Alternatives Considered

### 1. No Authentication

**Pros**:
- Simplest possible
- Zero client-side changes

**Cons**:
- Open to abuse (spam, resource exhaustion)
- No usage tracking or rate limiting foundation
- Unprofessional for production API

**Rejected because**: Fails to demonstrate production hygiene; unacceptable for public API.

### 2. OAuth 2.0 / OpenID Connect

**Pros**:
- Industry standard
- Fine-grained scopes and permissions
- Token expiration and refresh flows
- Supports multiple clients with different access levels

**Cons**:
- Massive complexity (authorization server, token validation, JWKS endpoints)
- Requires identity provider (Auth0, Okta, or self-hosted Keycloak)
- Overkill for single-endpoint API
- Weeks of implementation time

**Rejected because**: Way beyond Phase 1 scope; YAGNI for MVP.

### 3. JWT (JSON Web Tokens)

**Pros**:
- Self-contained (claims embedded in token)
- Stateless (no server-side session storage)
- Can include expiration (`exp` claim)
- Standard `Authorization: Bearer <jwt>` header

**Cons**:
- Requires token signing (manage secret keys or certificates)
- Token validation overhead (signature verification, expiration checks)
- Revocation is hard (need deny-list or short TTL + refresh flow)
- More complex than static API key for single-client use case

**Rejected because**: Adds complexity without commensurate benefit for Phase 1.

### 4. HTTP Basic Authentication

**Pros**:
- Simple, built into HTTP spec
- Supported by all clients (curl, browsers, fetch)

**Cons**:
- Sends credentials in every request (even with HTTPS, unnecessary exposure)
- Username/password model awkward for API clients (would use dummy username)
- No standard way to rotate without changing both username and password

**Rejected because**: API keys are more flexible and common for API authentication.

### 5. Gateway-Level Authentication (Render Blueprint + API Gateway)

**Pros**:
- Authentication handled outside application code
- Centralized policy enforcement
- Could add authentication to multiple services uniformly

**Cons**:
- Requires API gateway service (Kong, Tyk, AWS API Gateway)
- Render free tier doesn't include managed API gateway
- Adds deployment complexity
- Still need to choose auth mechanism (OAuth, API keys, etc.)

**Rejected because**: Render free tier limitation; can revisit if service grows.

### 6. IP Allow-List

**Pros**:
- Simple (check `X-Forwarded-For` header)
- No key distribution

**Cons**:
- IPs change (especially for mobile clients, home networks)
- Brittle (client IP change breaks access)
- No per-client identification
- Doesn't work for public clients (e.g., reviewers accessing demo)

**Rejected because**: Too restrictive; IP-based allowlisting is fragile.

## Implementation

### Middleware (ASP.NET Core)

```csharp
// Program.cs - Register before routing
app.Use(async (context, next) =>
{
    // Skip authentication in Development
    var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
    if (env.IsDevelopment())
    {
        await next();
        return;
    }

    // Get configured API key from settings
    var configuredKey = context.RequestServices
        .GetRequiredService<IConfiguration>()
        ["Security:ApiKey"];

    // Get provided API key from request header
    var providedKey = context.Request.Headers["X-API-Key"].FirstOrDefault();

    // Allow request if no key configured (fail-open) OR keys match
    if (string.IsNullOrWhiteSpace(configuredKey) || providedKey == configuredKey)
    {
        await next();
    }
    else
    {
        // Reject with 401 Unauthorized
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "UNAUTHORIZED",
                message = "Invalid or missing API key. Include 'X-API-Key' header."
            }
        });
    }
});
```

### Configuration

```json
// appsettings.Production.json
{
  "Security": {
    "ApiKey": "REPLACE_WITH_STRONG_RANDOM_KEY"
  }
}
```

**Render Environment Variable**: `Security__ApiKey=<generated-key>`

### Client Usage (React)

```typescript
// src/api/parse.ts
const API_KEY = import.meta.env.VITE_API_KEY; // Set in .env.production

export const parseText = async (text: string) => {
  const response = await fetch(`${API_BASE_URL}/api/v1/parse`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(API_KEY && { 'X-API-Key': API_KEY }), // Only add in production
    },
    body: JSON.stringify({ text }),
  });

  if (response.status === 401) {
    throw new Error('Unauthorized: Invalid API key');
  }

  return response.json();
};
```

### Testing

```bash
# Development (no key required)
curl -X POST http://localhost:5001/api/v1/parse \
  -H "Content-Type: application/json" \
  -d '{"text":"<total>100</total>"}'

# Production (key required)
curl -X POST https://flowingly-api.onrender.com/api/v1/parse \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-secret-key-here" \
  -d '{"text":"<total>100</total>"}'

# Production without key (401 error)
curl -X POST https://flowingly-api.onrender.com/api/v1/parse \
  -H "Content-Type: application/json" \
  -d '{"text":"<total>100</total>"}'
# Response: {"error":{"code":"UNAUTHORIZED","message":"Invalid or missing API key..."}}
```

## Key Rotation Procedure

1. **Generate New Key**: `openssl rand -base64 32`
2. **Update Render Environment Variable**: Set `Security__ApiKey` to new key
3. **Redeploy Application**: Trigger deployment (or Render auto-deploys on env var change)
4. **Update Clients**: Distribute new key to React UI config, QA team, reviewers
5. **Monitor**: Check logs for 401 errors (indicates clients using old key)
6. **Verify**: Test API with new key

**Recommended Cadence**: Rotate every 90 days or immediately if key suspected compromised.

## Future Enhancements (Out of Scope for Phase 1)

1. **Multiple API Keys**: Assign unique keys to each client; track usage per key
2. **Key Permissions**: Different keys with different endpoint access (e.g., read-only vs read-write)
3. **Automatic Expiration**: Keys expire after N days; clients must request new keys
4. **API Key Management UI**: Self-service portal for clients to generate/revoke keys
5. **Upgrade to OAuth 2.0**: If service grows to multiple APIs or external integrations
6. **Rate Limiting Per Key**: Enforce different rate limits for different API keys

## References

- PRD + Technical Specification (v0.2), Section 7: API Key Middleware
- PRD + Technical Specification (v0.2), Section 17: Open Questions (Question 3 - Auth)
- OWASP API Security Project: https://owasp.org/www-project-api-security/
- API Key Best Practices: https://cloud.google.com/endpoints/docs/openapi/when-why-api-key
- ASP.NET Core Middleware: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/

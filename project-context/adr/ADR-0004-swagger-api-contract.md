# ADR-0004: Swagger/OpenAPI for API Contract and Onboarding

**Status**: Accepted
**Date**: 2025-10-05
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service exposes a REST API for parsing text content. The API needs:

1. **Documentation**: Developers (internal team, reviewers, future maintainers) must understand:
   - Available endpoints (`POST /api/v1/parse`)
   - Request schema (what fields, types, required vs optional)
   - Response schema (success and error formats)
   - Example requests/responses
   - Error codes and their meanings

2. **Developer Experience**: Team members should be able to:
   - Explore the API interactively without writing code
   - Test endpoints directly from a browser
   - Understand contracts quickly during onboarding

3. **Contract Consistency**: As the API evolves:
   - Request/response DTOs change (new fields, validation rules)
   - Documentation must stay in sync with code
   - Manual documentation (markdown, wikis) often drifts from reality

4. **Client Code Generation** (future): Potential to generate TypeScript client for React app, or C# client for other services

5. **Handover/Review**: The test deliverable will be reviewed by senior engineers who need to understand the API surface quickly

The project emphasizes "easy to understand, maintain, and extend" (G5) and "clear documentation" for handover.

## Decision

**Adopt Swagger/OpenAPI 3.0 for API documentation and contract specification.**

Specifically:
- Use **Swashbuckle.AspNetCore** library for .NET integration
- Expose Swagger UI at `/swagger` in development environment
- Generate OpenAPI JSON specification at `/swagger/v1/swagger.json`
- Annotate API endpoints and DTOs with XML comments and attributes
- Disable Swagger UI in production (or protect with authentication)

## Consequences

### Positive

1. **Interactive Documentation**: Developers can:
   - Browse all endpoints in a web UI
   - See request/response schemas with examples
   - Execute requests directly (click "Try it out")
   - View actual responses and status codes

2. **Single Source of Truth**: API contracts defined in code (DTOs, attributes) auto-generate documentation
   - No separate markdown files to maintain
   - Schema changes reflect immediately in Swagger UI
   - Impossible for docs to drift from implementation

3. **Onboarding Acceleration**:
   - New developer runs app, navigates to `/swagger`, sees entire API surface
   - Understands data structures (expense fields, validation rules) visually
   - Can test happy/error paths without Postman or curl

4. **Review-Friendly**: Code reviewers can:
   - Check API design in Swagger UI
   - Validate error handling (do all 400 errors have clear codes?)
   - Ensure response consistency

5. **Client Generation Ready**: OpenAPI spec can generate:
   - TypeScript client for React app (via `openapi-generator` or `swagger-codegen`)
   - C# client for future microservices
   - Ensures type-safe API consumption

6. **Versioning Support**: Swagger can document multiple API versions (v1, v2) if/when API evolves

7. **Standard Format**: OpenAPI is industry-standard; many tools consume it (Postman, Insomnia, API gateways)

### Negative

1. **Dependency**: Adds `Swashbuckle.AspNetCore` NuGet package (~1MB)

2. **Build Time**: Minimal overhead to generate OpenAPI spec on startup

3. **XML Comments Requirement**: For rich documentation, need XML comments on DTOs/endpoints
   - Example: `/// <summary>The total amount including tax</summary>`
   - Requires enabling XML documentation generation in `.csproj`

4. **Exposure Risk**: If Swagger UI accidentally enabled in production, exposes API schema
   - Mitigation: Disable in production or add authentication

5. **Learning Curve**: Developers must learn Swagger annotations (`[SwaggerOperation]`, `[SwaggerResponse]`)
   - Minimal learning; most documentation auto-generated from types

### Mitigation Strategies

- **Production Safety**: Conditional registration:
  ```csharp
  if (app.Environment.IsDevelopment())
  {
      app.UseSwagger();
      app.UseSwaggerUI();
  }
  ```
- **XML Comments**: Enable in `.csproj`:
  ```xml
  <PropertyGroup>
      <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
  ```
- **Annotations**: Use attributes for clarity:
  ```csharp
  [SwaggerOperation(Summary = "Parse text content", Description = "...")]
  [SwaggerResponse(200, "Success", typeof(ParseResponse))]
  [SwaggerResponse(400, "Validation error", typeof(ErrorResponse))]
  ```

## Alternatives Considered

### 1. Manual Markdown Documentation

**Example**: `docs/API.md` with endpoint descriptions, example requests/responses.

**Pros**:
- No dependencies
- Full control over formatting

**Cons**:
- Drifts from code (request schema changes, docs not updated)
- Not interactive (developers need Postman/curl separately)
- High maintenance burden
- No client generation

**Rejected because**: Manual docs inevitably become stale; Swagger is self-maintaining.

### 2. Postman Collection

**Pros**:
- Interactive testing
- Shareable collection file

**Cons**:
- Separate artifact from code
- Must be manually updated when API changes
- Requires Postman installed
- Not browsable in browser

**Rejected because**: Swagger UI provides browser-based testing without additional tools.

### 3. NSwag (Alternative to Swashbuckle)

**Pros**:
- More features (client generation built-in, TypeScript strongly typed)
- Can generate C# clients

**Cons**:
- More complex configuration
- Overkill for Phase 1

**Considered but not chosen**: Swashbuckle is simpler and sufficient; NSwag can replace it later if client generation is critical.

### 4. API Blueprint or RAML

Alternative API specification formats.

**Pros**:
- Human-readable markdown-based specs

**Cons**:
- Less tooling support than OpenAPI
- Separate from code (manual maintenance)
- Smaller ecosystem

**Rejected because**: OpenAPI is industry standard; Swagger UI is widely used.

### 5. No API Documentation

Just rely on code and tests.

**Pros**:
- Zero effort

**Cons**:
- Fails "easy to understand and extend" requirement (G5)
- Poor handover experience for reviewers
- Slows onboarding

**Rejected because**: Documentation is explicit requirement.

## Implementation Guidelines

### Installation

```bash
dotnet add package Swashbuckle.AspNetCore
```

### Configuration (Program.cs)

**Key Methods**:
1. **`AddEndpointsApiExplorer()`**: Enables API endpoint discovery for Minimal APIs (required for Swagger to find endpoints)
2. **`AddSwaggerGen()`**: Configures OpenAPI document generation
3. **`UseSwagger()`**: Serves OpenAPI JSON at `/swagger/v1/swagger.json`
4. **`UseSwaggerUI()`**: Serves interactive Swagger UI at `/swagger`

```csharp
// Add Swagger services
builder.Services.AddEndpointsApiExplorer(); // Required for Minimal API endpoint discovery
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Flowingly Parsing Service API",
        Version = "v1",
        Description = "API for parsing free-form text into structured expense claims"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Enable Swagger UI (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flowingly Parsing Service API v1");
        options.RoutePrefix = "swagger"; // Access at /swagger
    });
}
```

### Annotating Endpoints

```csharp
app.MapPost("/api/v1/parse", async (ParseRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new ParseMessageCommand(request.Text, request.TaxRate, request.Currency));
    return result.Success ? Results.Ok(result.Data) : Results.BadRequest(result.Error);
})
.WithName("ParseMessage")
.WithOpenApi(operation => new(operation)
{
    Summary = "Parse text content for expense claims",
    Description = "Accepts free-form text with inline tags or XML islands, extracts expense data, validates required fields, computes NZ GST tax breakdown, and returns normalized JSON. Non-expense content stored as Other/Unprocessed."
})
.Produces<ParseResponse>(200)
.Produces<ErrorResponse>(400);
```

### DTO with XML Comments

```csharp
/// <summary>
/// Request to parse text content
/// </summary>
public record ParseRequest
{
    /// <summary>
    /// The raw text content to parse (required)
    /// </summary>
    [Required]
    public string Text { get; init; }

    /// <summary>
    /// Tax rate to apply (e.g., 0.15 for NZ GST). Defaults to configured rate if not provided.
    /// </summary>
    public decimal? TaxRate { get; init; }

    /// <summary>
    /// Currency code (e.g., "NZD"). Defaults to configured currency if not provided.
    /// </summary>
    public string? Currency { get; init; }
}
```

### Accessing Swagger UI

- **Development**: Navigate to `https://localhost:5001/swagger`
- **OpenAPI Spec**: `https://localhost:5001/swagger/v1/swagger.json`

## Usage in Development Workflow

1. **Developer starts coding**: Implements `ParseMessageCommand`
2. **Annotates DTOs**: Adds XML comments to `ParseRequest`, `ParseResponse`
3. **Runs app**: `dotnet run`
4. **Opens browser**: `https://localhost:5001/swagger`
5. **Tests endpoint**: Clicks "Try it out", enters sample text, sees response
6. **Validates error handling**: Tests invalid input, sees 400 error with clear error code

## M0→M2 Scope Guardrails

Per PRD v0.3 (Section 7, Section 18), the **M0→M2 scope** for Swagger is:
- ✅ **Basic Swagger UI**: Interactive endpoint exploration at `/swagger`
- ✅ **OpenAPI JSON Spec**: Generated at `/swagger/v1/swagger.json`
- ✅ **DTO Annotations**: XML comments on request/response models
- ✅ **Endpoint Documentation**: Summary, description, and response codes

**Out of Scope for M0→M2** (deferred to M3+ backlog):
- ❌ **Enhanced Swagger Examples**: Detailed example requests/responses for every scenario
- ❌ **Client Generation**: TypeScript client for React app
- ❌ **Multiple API Versions**: v1/v2 side-by-side documentation
- ❌ **ReDoc UI**: Alternative UI styling

**Rationale**: Basic Swagger UI provides sufficient documentation for core delivery (M0→M2). Enhanced examples and client generation are valuable enhancements but not critical for MVP validation. Focus on core functionality first, polish later.

## Future Enhancements (Post M0→M2)

### 1. Enhanced Swagger Examples

Add rich examples for every scenario:
```csharp
options.SwaggerDoc("v1", new OpenApiInfo
{
    // ... existing config
});

// Add example schemas
options.ExampleFilters();
options.OperationFilter<ExampleOperationFilter>();
```

**Example Response Documentation**:
```csharp
[SwaggerResponse(200, "Expense parsed successfully", typeof(ExpenseResponse),
    Example = @"{
        'classification': 'expense',
        'expense': {
            'vendor': 'Mojo Coffee',
            'total': 120.50,
            'totalExclTax': 104.78,
            'salesTax': 15.72
        }
    }")]
```

**Priority**: M3+ (nice-to-have, not critical for core functionality)

### 2. Client Generation

Generate TypeScript client for React:
```bash
npx @openapitools/openapi-generator-cli generate \
  -i https://localhost:5001/swagger/v1/swagger.json \
  -g typescript-fetch \
  -o ./client/src/api
```

**Priority**: M3+ (manual fetch calls sufficient for MVP)

### 3. Authentication in Production

If Swagger enabled in prod, add JWT auth:
```csharp
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
```

**Priority**: M4+ (production hardening, not needed for local dev/demo)

### 4. Multiple Versions

Document v1 and v2 side-by-side when API evolves.

**Priority**: M5+ (only needed if API versioning becomes necessary)

### 5. ReDoc Alternative UI

Replace Swagger UI with ReDoc for cleaner visuals.

**Priority**: M3+ (aesthetic improvement, not functional requirement)

## References

- PRD + Technical Specification v0.3, Section 7: Swagger (OpenAPI) — What & Why
- PRD v0.3, Section 18: M0→M2 Priority Guardrails (Swagger examples deferred)
- Swashbuckle.AspNetCore: https://github.com/domaindrivendev/Swashbuckle.AspNetCore
- OpenAPI Specification: https://swagger.io/specification/
- Swagger UI: https://swagger.io/tools/swagger-ui/

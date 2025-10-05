# ADR-0007: Response Contract Design

**Status**: Accepted
**Date**: 2025-10-06
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service processes free-form text and extracts structured data. The API must return different response structures depending on the classification of the input content:

1. **Expense**: Structured expense data with normalized fields (vendor, total, tax breakdown, etc.)
2. **Other/Unprocessed**: Raw tag content that doesn't match expense patterns, stored for future processing

Key considerations:
- **API consumers** (React UI, future clients) need clear, predictable response structures
- **Classification** determines response content (expense vs. other)
- **Type safety** is important for client-side TypeScript integration
- **Payload size** should be minimized (avoid unnecessary nulls or empty blocks)

The PRD v0.3 (Section 4.1, Section 6 Document History) specifies that responses must be **specific to classification** (expense XOR other, never both).

### Alternative Approaches

**Option A: Specific Responses (expense XOR other)**
```json
// Expense response
{
  "classification": "expense",
  "expense": { /* expense fields */ },
  "meta": { /* metadata */ }
}

// Other response
{
  "classification": "other",
  "other": { /* raw tags */ },
  "meta": { /* metadata */ }
}
```

**Option B: Combined Response (both blocks always present)**
```json
{
  "classification": "expense",
  "expense": { /* expense fields */ },  // populated if classification=expense
  "other": null,                        // null if classification=expense
  "meta": { /* metadata */ }
}
```

**Trade-off**: Specificity vs. flexibility

## Decision

**Responses are specific to classification (expense XOR other, never both).**

Each response will include:
1. **`classification`** field (enum: `"expense"` | `"other"`)
2. **Classification-specific block** (`expense` OR `other`, never both)
3. **`meta`** block (common metadata: correlation ID, processing time, warnings)

### Response Structure

#### Expense Response

```json
{
  "classification": "expense",
  "expense": {
    "vendor": "Mojo Coffee",
    "description": "Team lunch meeting",
    "total": 120.50,
    "totalExclTax": 104.78,
    "salesTax": 15.72,
    "costCentre": "DEV-TEAM",
    "date": "2024-10-05",
    "time": "12:30",
    "taxRate": 0.15
  },
  "meta": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "processingTimeMs": 45,
    "warnings": []
  }
}
```

#### Other Response

```json
{
  "classification": "other",
  "other": {
    "rawTags": {
      "reservation_date": "2024-12-25",
      "party_size": "6",
      "venue": "The French Café"
    }
  },
  "meta": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440001",
    "processingTimeMs": 12,
    "warnings": ["Non-expense content stored for future processing"]
  }
}
```

### TypeScript Discriminated Union

This structure enables **type-safe client code**:

```typescript
type ParseResponse = ExpenseResponse | OtherResponse;

interface ExpenseResponse {
  classification: "expense";
  expense: {
    vendor: string;
    description: string;
    total: number;
    totalExclTax: number;
    salesTax: number;
    costCentre: string;
    date: string;
    time: string | null;
    taxRate: number;
  };
  meta: Metadata;
}

interface OtherResponse {
  classification: "other";
  other: {
    rawTags: Record<string, string>;
  };
  meta: Metadata;
}

interface Metadata {
  correlationId: string;
  processingTimeMs: number;
  warnings: string[];
}

// Usage (type-safe!)
function handleResponse(response: ParseResponse) {
  if (response.classification === "expense") {
    console.log(response.expense.vendor); // ✅ TypeScript knows expense exists
    // console.log(response.other);       // ❌ TypeScript error: other doesn't exist
  } else {
    console.log(response.other.rawTags);  // ✅ TypeScript knows other exists
    // console.log(response.expense);     // ❌ TypeScript error: expense doesn't exist
  }
}
```

## Consequences

### Positive

1. **Clear Contracts**: Each classification has an explicit, unambiguous response shape
2. **Smaller Payloads**: No null expense/other blocks; only relevant data is returned
3. **Type Safety**: TypeScript discriminated unions provide compile-time guarantees
4. **Domain Alignment**: Response structure mirrors business reality (content is either expense or other, not both)
5. **Self-Documenting**: `classification` field clearly indicates response type
6. **Future Extensibility**: Easy to add new classifications (e.g., `"reservation"`) without breaking existing clients

### Negative

1. **Client Complexity**: Clients must handle two response shapes (requires conditional logic)
2. **Documentation Overhead**: Must document both response types in Swagger/OpenAPI
3. **No Single Response Type**: Cannot treat all responses uniformly without type checking

### Mitigation

- **OpenAPI Schema**: Use `oneOf` discriminator to document both response types
- **Client Libraries**: Provide TypeScript client with discriminated union types
- **Example Responses**: Include examples for both classifications in Swagger UI

## Alternatives Considered

### Alternative 1: Combined Response (Both Blocks Always Present)

**Structure**:
```json
{
  "classification": "expense",
  "expense": { /* populated if expense */ },
  "other": null,  // null if not other
  "meta": { /* metadata */ }
}
```

**Pros**:
- Single response type (simpler client code structure)
- Clients can always access both fields (no conditional access)

**Cons**:
- **Confusing nullability**: Which field is null? Requires checking classification anyway
- **Payload bloat**: Always includes null block (wasted bytes)
- **Misleading type hints**: IDE autocomplete suggests both expense and other exist
- **Poor type safety**: TypeScript can't prevent accessing null expense/other without runtime checks

**Rejected because**: Confusing nullability and payload bloat outweigh convenience.

### Alternative 2: Separate Endpoints (`/parse/expense`, `/parse/other`)

**Structure**:
```
POST /api/v1/parse/expense  → ExpenseResponse
POST /api/v1/parse/other    → OtherResponse
```

**Pros**:
- No conditional response handling
- Clear endpoint-to-response mapping
- Easy to document (one endpoint = one response type)

**Cons**:
- **Classification is determined during parsing**, not known upfront by client
- Client must guess classification before calling API (not feasible)
- Forces clients to retry with different endpoint if classification is wrong
- Violates REST principles (resource type shouldn't determine URL when it's derived from content)

**Rejected because**: Classification is determined by the service during processing, not by the client beforehand.

### Alternative 3: Polymorphic Response (No Classification Field)

**Structure**:
```json
// Expense response (no classification field, client infers from presence of expense)
{
  "expense": { /* expense fields */ },
  "meta": { /* metadata */ }
}

// Other response (client infers from presence of other)
{
  "other": { /* raw tags */ },
  "meta": { /* metadata */ }
}
```

**Pros**:
- Slightly smaller payload (no classification field)

**Cons**:
- **Ambiguous**: Client must infer type by checking which field exists
- **Error-prone**: Easy to misinterpret response if fields are accidentally undefined
- **Poor developer experience**: Requires defensive field existence checks

**Rejected because**: Explicit `classification` field improves clarity and reduces client errors.

## Implementation

### DTO Design Pattern

**Base Response**:
```csharp
public abstract class ParseResponseBase
{
    public string Classification { get; protected set; }
    public MetadataDto Meta { get; set; }
}

public class MetadataDto
{
    public string CorrelationId { get; set; }
    public int ProcessingTimeMs { get; set; }
    public List<string> Warnings { get; set; }
}
```

**Expense Response**:
```csharp
public class ExpenseResponse : ParseResponseBase
{
    public ExpenseResponse()
    {
        Classification = "expense";
    }

    public ExpenseDto Expense { get; set; }
}

public class ExpenseDto
{
    public string Vendor { get; set; }
    public string Description { get; set; }
    public decimal Total { get; set; }
    public decimal TotalExclTax { get; set; }
    public decimal SalesTax { get; set; }
    public string CostCentre { get; set; }
    public string Date { get; set; }
    public string? Time { get; set; }
    public decimal TaxRate { get; set; }
}
```

**Other Response**:
```csharp
public class OtherResponse : ParseResponseBase
{
    public OtherResponse()
    {
        Classification = "other";
    }

    public OtherDto Other { get; set; }
}

public class OtherDto
{
    public Dictionary<string, string> RawTags { get; set; }
}
```

### OpenAPI Schema (Swagger)

```yaml
components:
  schemas:
    ParseResponse:
      oneOf:
        - $ref: '#/components/schemas/ExpenseResponse'
        - $ref: '#/components/schemas/OtherResponse'
      discriminator:
        propertyName: classification
        mapping:
          expense: '#/components/schemas/ExpenseResponse'
          other: '#/components/schemas/OtherResponse'

    ExpenseResponse:
      type: object
      required:
        - classification
        - expense
        - meta
      properties:
        classification:
          type: string
          enum: [expense]
        expense:
          $ref: '#/components/schemas/ExpenseDto'
        meta:
          $ref: '#/components/schemas/Metadata'

    OtherResponse:
      type: object
      required:
        - classification
        - other
        - meta
      properties:
        classification:
          type: string
          enum: [other]
        other:
          $ref: '#/components/schemas/OtherDto'
        meta:
          $ref: '#/components/schemas/Metadata'
```

## References

- PRD + Technical Specification v0.3, Section 4.1: API Contract
- PRD v0.3, Section 6: Document History (response structure change)
- ADR-0003: Processor Strategy Pattern (response building within processors)
- TypeScript Discriminated Unions: https://www.typescriptlang.org/docs/handbook/unions-and-intersections.html#discriminating-unions
- OpenAPI Discriminator: https://swagger.io/docs/specification/data-models/inheritance-and-polymorphism/

# ADR-0008: Parsing and Validation Rules

**Status**: Accepted
**Date**: 2025-10-06
**Deciders**: Technical Lead, Development Team

## Context

The Flowingly Parsing Service processes free-form text containing inline tags and XML islands. The PRD v0.3 (Section 4.2) introduced critical parsing and validation rules to ensure data integrity and prevent ambiguous or malformed inputs from producing incorrect results.

### Key Requirements

1. **Tag Integrity Validation**: Detect and reject overlapping or improperly nested tags
   - Example invalid: `<a><b></a></b>` (closing `a` before `b`)
   - Example valid: `<a><b></b></a>` (proper nesting)

2. **Tax Rate Precedence**: Clear resolution order when tax rate is provided via multiple sources
   - Request parameter > Config default > Error (if strict) or fallback 0.15

3. **Ambiguous Time Handling**: Reject unclear time formats to prevent incorrect data entry
   - Accept: `14:30`, `2:30 PM`, `14:30:00`
   - Reject: `230`, `2.30`, unclear formats

These rules are critical for **data correctness** and **preventing silent failures** that could lead to incorrect expense submissions.

## Decision

### 1. Stack-Based Tag Validation (Not Just Balance Checking)

**Decision**: Implement a **stack-based parser** with nesting validation, not just opening/closing tag balance.

**Rationale**: Simple regex or balance counting (`<a>` count == `</a>` count) cannot detect overlapping tags like `<a><b></a></b>`.

**Algorithm**:
```
1. Initialize empty stack
2. For each tag in input:
   - If opening tag <X>: push X onto stack
   - If closing tag </X>:
     - If stack is empty → ERROR: "Unexpected closing tag </X>"
     - If stack.top != X → ERROR: "Overlapping tags: expected </stack.top>, found </X>"
     - Else: pop stack
3. If stack is not empty after processing → ERROR: "Unclosed tags: [stack contents]"
```

**Examples**:

| Input | Validation Result |
|-------|-------------------|
| `<a><b></b></a>` | ✅ Valid (proper nesting) |
| `<a><b></a></b>` | ❌ Invalid (overlapping: expected `</b>`, found `</a>`) |
| `<total>120</total>` | ✅ Valid |
| `<total>120` | ❌ Invalid (unclosed tag: `total`) |
| `</total>` | ❌ Invalid (unexpected closing tag) |

### 2. Tax Rate Precedence Chain

**Decision**: Use explicit precedence order with configurable strict mode.

**Precedence Order**:
1. **Request Parameter** (`tax_rate` in POST body) — highest priority
2. **Configuration Default** (`appsettings.json` → `TaxRate: 0.15`)
3. **Strict Mode Behavior**:
   - If `StrictTaxRate = true` → **Reject request** with 400 error ("Tax rate required")
   - If `StrictTaxRate = false` → **Fallback to 0.15** (NZ GST default), log warning

**Implementation**:
```csharp
public decimal ResolveTaxRate(decimal? requestTaxRate, IConfiguration config)
{
    // 1. Request parameter (highest priority)
    if (requestTaxRate.HasValue)
        return requestTaxRate.Value;

    // 2. Config default
    var configTaxRate = config.GetValue<decimal?>("TaxRate");
    if (configTaxRate.HasValue)
        return configTaxRate.Value;

    // 3. Strict mode or fallback
    var strictMode = config.GetValue<bool>("StrictTaxRate");
    if (strictMode)
        throw new ValidationException("Tax rate is required (strict mode enabled)");

    // Fallback to NZ GST default
    _logger.LogWarning("Tax rate not provided, using fallback: 0.15");
    return 0.15m;
}
```

**Configuration Example**:
```json
{
  "TaxRate": 0.15,
  "StrictTaxRate": false  // true = require explicit tax rate, false = allow fallback
}
```

### 3. Time Parsing Whitelist (Explicit Formats Only)

**Decision**: Use **whitelist-based time parsing** with explicit format validation. Reject ambiguous inputs.

**Rationale**: Ambiguous formats like `230` (2:30? 23:0? 2300 hours?) lead to incorrect data. Better to reject than guess.

**Accepted Formats**:
- `HH:mm` (24-hour, e.g., `14:30`)
- `HH:mm:ss` (24-hour with seconds, e.g., `14:30:00`)
- `h:mm tt` (12-hour with AM/PM, e.g., `2:30 PM`)
- `h:mm:ss tt` (12-hour with seconds and AM/PM, e.g., `2:30:00 PM`)

**Rejected Formats**:
- `230` (ambiguous: 2:30 or 23:0?)
- `2.30` (unclear separator)
- `1430` (no separator)
- Any format not in whitelist

**Implementation**:
```csharp
public TimeSpan? ParseTime(string input)
{
    var formats = new[]
    {
        "HH:mm",
        "HH:mm:ss",
        "h:mm tt",
        "h:mm:ss tt"
    };

    if (TimeSpan.TryParseExact(input, formats, CultureInfo.InvariantCulture, out var result))
        return result;

    _logger.LogWarning("Ambiguous time format rejected: {Input}", input);
    return null;  // Ignore unclear time, do not guess
}
```

**Behavior on Rejection**:
- Log warning: `"Ambiguous time format rejected: {input}"`
- Set `time` field to `null` in response (time is optional)
- Do NOT throw error (time is not required for expense validation)

## Consequences

### Positive

1. **Robust Validation**: Stack-based parser catches all nesting violations, not just balance errors
2. **Clear Error Messages**: Specific errors (e.g., "Expected `</b>`, found `</a>`") help users fix issues
3. **No Silent Failures**: Tax rate precedence and strict mode prevent ambiguous tax calculations
4. **Data Accuracy**: Time whitelist prevents incorrect time entries from ambiguous formats
5. **Configurable Strictness**: `StrictTaxRate` allows production strictness with dev/test flexibility

### Negative

1. **More Complex Parser Logic**: Stack-based validation is more complex than regex balance checking
2. **More Test Cases Required**: Must test overlapping tags, all precedence scenarios, all time formats
3. **User Friction**: Rejecting ambiguous times may frustrate users who expect permissive parsing
4. **Maintenance Overhead**: Time format whitelist must be updated if new formats are required

### Mitigation

- **Comprehensive Tests**: BDD scenarios cover all edge cases (see ADR-0010)
- **Clear Error Messages**: Help users quickly identify and fix validation issues
- **Documentation**: README includes accepted time formats and validation rules
- **Flexible Config**: `StrictTaxRate` allows different behavior per environment

## Alternatives Considered

### Alternative 1: Regex-Only Tag Validation

**Approach**: Use regex to count opening and closing tags, ensure balance.

```regex
<(\w+)>  → count opening tags
</(\w+)> → count closing tags
Assert: count(<a>) == count(</a>) for all tags
```

**Pros**:
- Simple implementation
- Fast execution

**Cons**:
- **Cannot detect overlapping tags**: `<a><b></a></b>` would pass validation
- No nesting depth tracking
- Poor error messages (only "unbalanced tags", not specific issue)

**Rejected because**: Overlapping tags must be detected to prevent malformed data.

### Alternative 2: Always Fallback to Default Tax Rate (No Strict Mode)

**Approach**: Always use 0.15 if tax rate not provided, never error.

**Pros**:
- Simpler logic (no strict mode config)
- More permissive (fewer validation errors)

**Cons**:
- **Hides user errors**: If user forgets tax rate, system silently uses 0.15 (may be incorrect)
- Incorrect tax calculations for non-NZ expenses
- No way to enforce explicit tax rate in production

**Rejected because**: Silent fallback can lead to incorrect data; strict mode provides safety.

### Alternative 3: Permissive Time Parsing (Accept All Formats)

**Approach**: Use `DateTime.TryParse` or `TimeSpan.TryParse` with permissive culture settings.

**Pros**:
- Accepts wide range of formats
- Fewer rejected inputs
- Better user experience (less friction)

**Cons**:
- **Ambiguous inputs lead to incorrect data**: `230` could be parsed as `2:30` or `23:00`
- Different cultures parse differently (e.g., `2.30` in some locales means `2:30`)
- Silent misinterpretation is worse than rejection

**Rejected because**: Data correctness is more important than accepting all inputs. Better to reject than misinterpret.

### Alternative 4: XML Schema Validation (XSD)

**Approach**: Define XML schema, validate using `XmlReader` with schema validation.

**Pros**:
- Industry-standard validation
- Rich validation rules (types, constraints, patterns)

**Cons**:
- Overkill for inline tags (not full XML documents)
- Requires learning XSD syntax
- Less flexible for hybrid content (free-form text + tags)
- More complex setup

**Rejected because**: Stack-based parser is simpler and sufficient for inline tag validation.

## Implementation

### Stack-Based Tag Validator

```csharp
public class TagValidator : ITagValidator
{
    public ValidationResult Validate(string content)
    {
        var stack = new Stack<string>();
        var regex = new Regex(@"<(/?)(\w+)>");

        foreach (Match match in regex.Matches(content))
        {
            var isClosing = match.Groups[1].Value == "/";
            var tagName = match.Groups[2].Value;

            if (!isClosing)
            {
                // Opening tag: push onto stack
                stack.Push(tagName);
            }
            else
            {
                // Closing tag: validate nesting
                if (stack.Count == 0)
                    return ValidationResult.Failure($"Unexpected closing tag </{tagName}>");

                var expected = stack.Pop();
                if (expected != tagName)
                    return ValidationResult.Failure($"Overlapping tags: expected </{expected}>, found </{tagName}>");
            }
        }

        if (stack.Count > 0)
            return ValidationResult.Failure($"Unclosed tags: {string.Join(", ", stack)}");

        return ValidationResult.Success();
    }
}
```

### Tax Rate Resolution Chain

```csharp
public class TaxRateResolver : ITaxRateResolver
{
    private readonly IConfiguration _config;
    private readonly ILogger<TaxRateResolver> _logger;

    public decimal Resolve(decimal? requestTaxRate)
    {
        // 1. Request parameter
        if (requestTaxRate.HasValue)
            return requestTaxRate.Value;

        // 2. Config default
        var configTaxRate = _config.GetValue<decimal?>("TaxRate");
        if (configTaxRate.HasValue)
            return configTaxRate.Value;

        // 3. Strict mode or fallback
        var strictMode = _config.GetValue<bool>("StrictTaxRate");
        if (strictMode)
            throw new ValidationException("Tax rate is required");

        _logger.LogWarning("Tax rate not provided, using fallback: 0.15");
        return 0.15m;
    }
}
```

### Time Format Whitelist

```csharp
public class TimeParser : ITimeParser
{
    private static readonly string[] AcceptedFormats = { "HH:mm", "HH:mm:ss", "h:mm tt", "h:mm:ss tt" };
    private readonly ILogger<TimeParser> _logger;

    public TimeSpan? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (TimeSpan.TryParseExact(input, AcceptedFormats, CultureInfo.InvariantCulture, out var result))
            return result;

        _logger.LogWarning("Ambiguous time format rejected: {Input}", input);
        return null;
    }
}
```

## Testing Strategy

### BDD Scenarios (see ADR-0010 for full test plan)

**Overlapping Tag Validation**:
- GIVEN content with overlapping tags `<a><b></a></b>`
- WHEN parsing
- THEN validation fails with error "Overlapping tags: expected `</b>`, found `</a>`"

**Tax Rate Precedence**:
- GIVEN request with `tax_rate: 0.10` AND config default `0.15`
- WHEN resolving tax rate
- THEN result is `0.10` (request wins)

- GIVEN request with no `tax_rate` AND config default `0.15`
- WHEN resolving tax rate
- THEN result is `0.15` (config default)

- GIVEN request with no `tax_rate` AND no config default AND `StrictTaxRate: true`
- WHEN resolving tax rate
- THEN throws ValidationException

**Ambiguous Time Handling**:
- GIVEN content with time `"230"`
- WHEN parsing time
- THEN result is `null`, warning logged

- GIVEN content with time `"14:30"`
- WHEN parsing time
- THEN result is `TimeSpan(14, 30, 0)`

## References

- PRD + Technical Specification v0.3, Section 4.2: Parsing Rules
- PRD v0.3, Section 6: Document History (overlapping tag validation, tax precedence, ambiguous time handling)
- ADR-0010: Test Strategy and Coverage (BDD scenarios for parsing rules)
- Stack-Based Parsing: https://en.wikipedia.org/wiki/Shunting-yard_algorithm
- C# TimeSpan.ParseExact: https://learn.microsoft.com/en-us/dotnet/api/system.timespan.parseexact

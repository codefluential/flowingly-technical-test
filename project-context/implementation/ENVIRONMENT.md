# Environment Configuration

Quick reference for environment-specific settings required for task execution.

## .NET 8 Configuration

### Installation Details
- **Location**: `/home/adarsh/.dotnet/dotnet`
- **Version**: 8.0.414
- **DOTNET_ROOT**: `$HOME/.dotnet`

### PATH Configuration Required

**Problem**: dotnet is NOT in the default PATH for new shell sessions.

**Solution**: ALL bash commands that use dotnet MUST export PATH first.

### Usage Pattern

```bash
# ALWAYS use this pattern for dotnet commands:
export PATH="$HOME/.dotnet:$PATH" && dotnet <command>
```

### Examples

#### Building Solution
```bash
export PATH="$HOME/.dotnet:$PATH" && dotnet build
```

#### Running Tests
```bash
export PATH="$HOME/.dotnet:$PATH" && dotnet test
export PATH="$HOME/.dotnet:$PATH" && dotnet test --filter "Category=Unit"
```

#### Running API
```bash
export PATH="$HOME/.dotnet:$PATH" && dotnet run --project src/Api
```

#### Migrations
```bash
export PATH="$HOME/.dotnet:$PATH" && dotnet ef migrations add MigrationName
export PATH="$HOME/.dotnet:$PATH" && dotnet ef database update
```

### For Task Files (JSON)

When creating task files, escape quotes properly:

```json
{
  "validation": {
    "command": "export PATH=\"$HOME/.dotnet:$PATH\" && dotnet test",
    "expected_output": "Test run successful"
  }
}
```

### For Agent Instructions

Include this in any agent prompt that will use dotnet:

```
IMPORTANT: dotnet is installed at /home/adarsh/.dotnet/dotnet.
ALL dotnet commands MUST be prefixed with: export PATH="$HOME/.dotnet:$PATH" &&

Example: export PATH="$HOME/.dotnet:$PATH" && dotnet build
```

---

## Node.js Configuration

Node.js and npm are in the default PATH and work without modification.

```bash
# These work directly:
node --version
npm install
npm run dev
```

---

## Verification

To verify environment setup:

```bash
# Check dotnet (with PATH export)
export PATH="$HOME/.dotnet:$PATH" && dotnet --version
# Expected: 8.0.414

# Check node
node --version
# Expected: v18+ or v20+

# Check npm
npm --version
# Expected: 9+ or 10+
```

---

## Common Issues

### "dotnet: command not found"

**Cause**: Forgot to export PATH before using dotnet

**Fix**: Add `export PATH="$HOME/.dotnet:$PATH" &&` before the command

**Wrong**:
```bash
dotnet build
```

**Correct**:
```bash
export PATH="$HOME/.dotnet:$PATH" && dotnet build
```

---

## Quick Reference Card

| Tool | Location | PATH Required? | Prefix Command |
|------|----------|----------------|----------------|
| dotnet | `/home/adarsh/.dotnet/dotnet` | ✅ YES | `export PATH="$HOME/.dotnet:$PATH" &&` |
| node | System PATH | ❌ NO | (none) |
| npm | System PATH | ❌ NO | (none) |
| git | System PATH | ❌ NO | (none) |

---

**Last Updated**: 2025-10-06
**Status**: Active configuration for all tasks

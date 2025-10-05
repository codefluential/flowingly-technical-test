# Serena MCP - Semantic Code Agent Toolkit Guide

**Date Created**: 2025-10-06
**Purpose**: Comprehensive guide to Serena MCP for semantic code analysis and manipulation
**Repository**: https://github.com/oraios/serena

---

## Table of Contents

1. [What is Serena MCP?](#what-is-serena-mcp)
2. [Installation](#installation)
3. [Project Setup](#project-setup)
4. [Available Tools](#available-tools)
5. [Usage Patterns](#usage-patterns)
6. [Configuration](#configuration)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)
9. [When to Use Serena](#when-to-use-serena)

---

## What is Serena MCP?

**Serena** is a powerful **Model Context Protocol (MCP) server** that transforms Large Language Models (LLMs) into advanced development assistants with **semantic code understanding** capabilities.

### Key Capabilities

- **Semantic Code Navigation**: Find symbols, classes, methods, references using Language Server Protocol (LSP)
- **Precise Code Editing**: Replace/insert code by symbol location (not line numbers or regex)
- **Multi-Language Support**: 20+ languages including Python, TypeScript, JavaScript, C#, Java, Go, Rust, PHP, Ruby, C++, Swift, Elixir, Terraform, Bash
- **Token-Efficient Retrieval**: Read only the code you need (method bodies, class definitions) without loading entire files
- **Reference Tracking**: Find all places where a symbol is used across the codebase

### Why Use Serena?

**Traditional Tools** (Read, Grep, Edit):
- Line-based editing (fragile when code changes)
- Full-file reads (wastes tokens)
- Regex-based search (misses semantic relationships)

**Serena**:
- Symbol-based editing (robust to line number changes)
- Targeted reads (only method bodies you need)
- Semantic search (find all references to a class/method)

### Ideal Use Cases

✅ **Great for:**
- Large, complex codebases with many files/classes
- Refactoring (renaming symbols, updating references)
- Understanding code structure (class hierarchies, method calls)
- Finding all usages of a function across the project

❌ **Not ideal for:**
- Greenfield projects with no code yet
- Small scripts (< 5 files)
- Documentation-only repositories
- Non-code files (markdown, JSON, YAML)

---

## Installation

Serena is typically installed as an MCP server in your development environment (Claude Code, Claude Desktop, etc.).

### Prerequisites

- **UV** (Python package manager): Install from https://docs.astral.sh/uv/
- **Supported language runtimes** (e.g., Node.js for TypeScript, .NET SDK for C#)

### Installation via UV (Recommended)

```bash
# Install Serena globally
uvx --from git+https://github.com/oraios/serena serena --help

# Verify installation
uvx --from git+https://github.com/oraios/serena serena --version
```

### Alternative: Docker

```bash
# Pull Serena Docker image
docker pull ghcr.io/oraios/serena:latest

# Run Serena via Docker
docker run -v $(pwd):/workspace ghcr.io/oraios/serena serena project index
```

### MCP Server Configuration

If using **Claude Desktop** or **Claude Code**, Serena is configured in the MCP settings file:

**Example MCP Config** (`claude_desktop_config.json` or similar):
```json
{
  "mcpServers": {
    "serena": {
      "command": "uvx",
      "args": [
        "--from",
        "git+https://github.com/oraios/serena",
        "serena"
      ]
    }
  }
}
```

---

## Project Setup

### Step 1: Verify Project Has Source Code

Serena requires **actual source code files** in supported languages. It won't activate for documentation-only projects.

**Supported Languages**:
- Python (`.py`)
- TypeScript/JavaScript (`.ts`, `.tsx`, `.js`, `.jsx`)
- C# (`.cs`)
- Java (`.java`)
- Go (`.go`)
- Rust (`.rs`)
- PHP (`.php`)
- Ruby (`.rb`)
- C++ (`.cpp`, `.h`)
- Swift (`.swift`)
- Elixir (`.ex`, `.exs`)
- Terraform (`.tf`)
- Bash (`.sh`)

**Check if you're ready**:
```bash
# From project root
find . -name "*.cs" -o -name "*.ts" -o -name "*.py" | head -5
```

If no source files exist, **wait until you scaffold your project** (e.g., `dotnet new` for .NET, `npm create vite@latest` for React).

---

### Step 2: Index the Project (Recommended for Performance)

Indexing accelerates Serena's semantic tools by pre-analyzing your codebase.

```bash
# From project root
uvx --from git+https://github.com/oraios/serena serena project index

# Output example:
# Indexing project: /home/user/my-project
# Language detected: csharp
# Indexed 42 files in 3.2s
```

**When to index**:
- After initial project scaffold
- After major code additions (new modules, libraries)
- If Serena tools feel slow

**Index location**: `.serena/index/` (gitignored by default)

---

### Step 3: Activate the Project

Before using Serena tools in Claude Code, **activate the project** by telling Claude:

```
Activate the project /home/adarsh/dev/codefluent/flowingly-technical-test
```

Or if previously activated:
```
Activate the project flowingly-technical-test
```

**What happens on activation**:
1. Serena creates `.serena/project.yml` if it doesn't exist
2. Project is registered in `~/.config/serena/serena_config.yml`
3. Project becomes the active context for all Serena tools

**Verify activation**:
```
Use the get_current_config tool to check active project
```

---

### Step 4: Create Manual Configuration (Optional)

If Serena can't auto-detect your project language, create `.serena/project.yml` manually:

```bash
mkdir -p .serena
```

**`.serena/project.yml` Example**:
```yaml
project_name: flowingly-technical-test
language: csharp  # or python, typescript, java, go, rust, etc.

# Optional: Custom paths
source_paths:
  - src/
  - lib/

# Optional: Ignore patterns
ignore_patterns:
  - "**/bin/**"
  - "**/obj/**"
  - "**/node_modules/**"
```

**Language Options**:
- `python`
- `typescript` (also covers JavaScript)
- `java`
- `csharp`
- `rust`
- `go`
- `ruby`
- `cpp`
- `php`
- `swift`
- `elixir`
- `terraform`
- `bash`

---

## Available Tools

Serena provides **semantic code tools** via MCP. All tools require an **active project**.

### File & Directory Navigation

#### `list_dir`
Lists files and directories (respects .gitignore).

```
List the files in src/Application/
```

**Parameters**:
- `relative_path`: Directory to list (`.` for root)
- `recursive`: Boolean (true for nested listing)
- `skip_ignored_files`: Boolean (default: false)

---

#### `find_file`
Finds files matching a pattern.

```
Find all files named *Controller.cs
```

**Parameters**:
- `file_mask`: Pattern with wildcards (`*.cs`, `*Test.java`)
- `relative_path`: Directory to search (`.` for root)

---

### Code Search

#### `search_for_pattern`
Searches for regex patterns in file contents.

```
Search for the pattern "public class.*Controller" in src/
```

**Parameters**:
- `substring_pattern`: Regex pattern
- `relative_path`: Directory to search
- `context_lines_before`: Lines of context before match
- `context_lines_after`: Lines of context after match
- `restrict_search_to_code_files`: Boolean (skip non-code files)
- `paths_include_glob`: Glob to include files (e.g., `*.cs`)
- `paths_exclude_glob`: Glob to exclude files (e.g., `*Test.cs`)

**Example**:
```
Search for "ITaxCalculator" in src/ with 2 lines of context before and after
```

---

### Semantic Code Analysis

#### `get_symbols_overview`
Gets high-level overview of symbols in a file (classes, methods, functions).

```
Get symbols overview for src/Domain/Services/TaxCalculator.cs
```

**Use Case**: First tool to call when exploring a new file. Shows structure without reading full implementation.

**Returns**: Symbol names, types (class/method/function), line ranges.

---

#### `find_symbol`
Finds symbols by name path (precise or fuzzy matching).

```
Find symbol "TaxCalculator/CalculateFromInclusive" in src/Domain/
```

**Parameters**:
- `name_path`: Symbol path (e.g., `ClassName/MethodName` or `/TopLevelClass`)
- `relative_path`: File or directory to search (optional)
- `depth`: How deep to retrieve children (0=symbol only, 1=immediate children)
- `include_body`: Boolean (include source code)
- `substring_matching`: Boolean (fuzzy name matching)
- `include_kinds`: LSP symbol kinds to include (e.g., `[5]` for classes, `[6]` for methods)
- `exclude_kinds`: LSP symbol kinds to exclude

**Name Path Patterns**:
- `MethodName`: Find any method named `MethodName` (anywhere)
- `ClassName/MethodName`: Find `MethodName` inside `ClassName`
- `/ClassName`: Find top-level class `ClassName` only (not nested)
- `/ClassName/MethodName`: Absolute path (top-level `ClassName`, method `MethodName`)

**LSP Symbol Kinds** (for filtering):
- `5` = Class
- `6` = Method
- `12` = Function
- `13` = Variable
- `14` = Constant
- `10` = Enum
- `11` = Interface

**Example**:
```
Find symbol "ExpenseProcessor" with depth 1 to see all methods
```

---

#### `find_referencing_symbols`
Finds all places where a symbol is referenced (usages).

```
Find all references to "ITaxCalculator" in src/
```

**Parameters**:
- `name_path`: Symbol to find references for
- `relative_path`: File containing the symbol (required)
- `include_kinds`: Filter referencing symbols by kind
- `exclude_kinds`: Exclude referencing symbols by kind

**Use Case**: Before refactoring/renaming, find all usages.

---

### Code Editing

#### `replace_symbol_body`
Replaces the entire body of a symbol (class, method, function).

```
Replace the body of TaxCalculator/CalculateFromInclusive
```

**Parameters**:
- `name_path`: Symbol to replace
- `relative_path`: File containing symbol
- `body`: New implementation (including signature)

**Important**: `body` includes the signature (e.g., `public decimal Calculate() { ... }`), NOT just the method contents.

---

#### `insert_after_symbol`
Inserts new code after a symbol definition.

```
Insert a new method after TaxCalculator/CalculateFromInclusive
```

**Parameters**:
- `name_path`: Symbol to insert after
- `relative_path`: File containing symbol
- `body`: Code to insert (starts on next line after symbol)

**Use Case**: Add new methods to a class.

---

#### `insert_before_symbol`
Inserts new code before a symbol definition.

```
Insert a using statement before the first class in the file
```

**Parameters**:
- `name_path`: Symbol to insert before
- `relative_path`: File containing symbol
- `body`: Code to insert (before symbol definition line)

**Use Case**: Add imports, using statements, or new classes.

---

### Memory & Context

#### `write_memory`
Stores information about the project for future sessions.

```
Write a memory about the parsing pipeline architecture
```

**Parameters**:
- `memory_name`: Meaningful name (e.g., `parsing-pipeline-overview`)
- `content`: Markdown content

**Stored in**: `.serena/memories/`

---

#### `read_memory`
Retrieves stored project information.

```
Read the memory "parsing-pipeline-overview"
```

**Use Case**: Recall architectural decisions, module boundaries, patterns used.

---

#### `list_memories`
Lists all available memory files.

```
List all memories
```

---

#### `delete_memory`
Deletes a memory file.

```
Delete the memory "outdated-architecture-notes"
```

---

### Configuration

#### `get_current_config`
Shows active project and configuration.

```
Get the current Serena configuration
```

**Returns**: Active project, available projects, tools enabled, contexts, modes.

---

#### `activate_project`
Activates a project (sets active context).

```
Activate the project /path/to/my-project
```

Or by name:
```
Activate the project my-project
```

---

#### `check_onboarding_performed`
Checks if project onboarding is complete.

```
Check if onboarding was performed for this project
```

---

#### `onboarding`
Runs project onboarding flow (initial setup, architecture capture).

```
Run onboarding for this project
```

---

### Thinking Tools (Internal Reflection)

#### `think_about_collected_information`
Reflects on whether collected info is sufficient/relevant.

**When to use**: After searching, reading symbols, etc. Serena prompts Claude to assess if more info is needed.

---

#### `think_about_task_adherence`
Reflects on whether work is on track with the task.

**When to use**: Before making edits, after long conversations.

---

#### `think_about_whether_you_are_done`
Reflects on task completion status.

**When to use**: When you think you've finished the task.

---

## Usage Patterns

### Pattern 1: Exploring a New File

```
1. Get symbols overview for src/Domain/Processors/ExpenseProcessor.cs
2. Find symbol "ExpenseProcessor" with depth 1 (to see all methods)
3. Find symbol "ExpenseProcessor/ProcessAsync" with include_body=true (to read implementation)
```

**Why**: Starts broad (overview), narrows to specific methods (targeted reads).

---

### Pattern 2: Refactoring a Method

```
1. Find symbol "TaxCalculator/CalculateFromInclusive" with include_body=true
2. Find all references to "TaxCalculator/CalculateFromInclusive"
3. Replace symbol body "TaxCalculator/CalculateFromInclusive" with new implementation
4. (If signature changed) Update all referencing symbols
```

**Why**: See current implementation → find usages → safely replace → update callers.

---

### Pattern 3: Adding a New Feature

```
1. Find symbol "ExpenseProcessor" with depth 1 (see existing methods)
2. Insert after symbol "ExpenseProcessor/ProcessAsync" (add new method)
3. Find symbol "ContentRouter" (to wire up new processor)
4. Replace symbol body "ContentRouter/RouteAsync" (add routing logic)
```

**Why**: Understand existing structure → insert new code → integrate with routing.

---

### Pattern 4: Understanding Call Chains

```
1. Find symbol "ITaxCalculator"
2. Find referencing symbols for "ITaxCalculator"
3. For each referencing symbol, find its body to see how it's used
```

**Why**: Trace dependencies and understand how interfaces are consumed.

---

## Configuration

### User-Level Config

**Location**: `~/.config/serena/serena_config.yml`

**Auto-generated** when you first activate a project.

**Example**:
```yaml
projects:
  - name: flowingly-technical-test
    path: /home/adarsh/dev/codefluent/flowingly-technical-test
    language: csharp
```

---

### Project-Level Config

**Location**: `.serena/project.yml`

**Auto-generated** on first activation, or create manually.

**Example**:
```yaml
project_name: flowingly-technical-test
language: csharp

# Optional: Custom source paths
source_paths:
  - src/
  - lib/

# Optional: Ignore patterns (beyond .gitignore)
ignore_patterns:
  - "**/bin/**"
  - "**/obj/**"
  - "**/node_modules/**"
  - "**/*.min.js"

# Optional: LSP settings
lsp:
  timeout: 30  # seconds
  max_file_size: 1048576  # bytes (1MB)
```

---

### .gitignore Recommendations

Add to `.gitignore`:
```gitignore
# Serena MCP
.serena/index/
.serena/memories/  # Optional: commit if you want to share team knowledge
```

---

## Best Practices

### 1. Activate Project at Start of Session

Always activate before using Serena tools:
```
Activate the project flowingly-technical-test
```

---

### 2. Use Semantic Tools for Code, Standard Tools for Docs

- **Serena**: `.cs`, `.ts`, `.py` files (classes, methods, functions)
- **Read/Edit**: `.md`, `.json`, `.yaml`, `.txt` files

---

### 3. Start with Overview, Then Drill Down

```
1. get_symbols_overview (see file structure)
2. find_symbol with depth=1 (see class methods)
3. find_symbol with include_body=true (read specific method)
```

**Avoid**: Reading entire files with `Read` when you only need one method.

---

### 4. Index Large Projects

If your codebase has >50 files:
```bash
uvx --from git+https://github.com/oraios/serena serena project index
```

Re-index after major code additions.

---

### 5. Use Memories for Architecture Notes

Store:
- Module boundaries
- Design patterns used
- Key abstractions (e.g., "Ports are in Domain/Interfaces, Adapters in Infrastructure")
- Onboarding notes for new developers

```
Write a memory "architecture-overview" with:
- Hexagonal architecture with Ports & Adapters
- Domain layer is pure C# (no framework dependencies)
- Infrastructure layer implements ports (EF Core, Postgres)
```

---

### 6. Check References Before Renaming

```
1. Find referencing symbols for "OldMethodName"
2. (Review all usages)
3. Replace symbol body "ClassName/OldMethodName" (rename + implementation)
4. Update all referencing symbols
```

---

### 7. Prefer Symbol Editing Over Line-Based Editing

**Instead of**:
```
Edit file.cs, replace lines 42-58 with...
```

**Do**:
```
Replace symbol body "ClassName/MethodName" with...
```

**Why**: Robust to line number changes when code is modified elsewhere.

---

## Troubleshooting

### Error: "No active project"

**Cause**: Project not activated.

**Fix**:
```
Activate the project /path/to/my-project
```

---

### Error: "No source files found"

**Cause**: Project has no code files yet (documentation-only).

**Fix**: Wait until you scaffold the project (e.g., `dotnet new`, `npm create vite`), then activate.

**Workaround**: Create `.serena/project.yml` manually with language specified.

---

### Slow Symbol Searches

**Cause**: Large project not indexed.

**Fix**:
```bash
uvx --from git+https://github.com/oraios/serena serena project index
```

---

### Symbol Not Found

**Cause**: Incorrect name path or file path.

**Fix**:
1. Use `get_symbols_overview` to see available symbols
2. Check exact spelling and casing
3. Try `substring_matching: true` for fuzzy search

---

### Serena Tools Not Available in Claude

**Cause**: MCP server not configured or not running.

**Fix**:
1. Check MCP config (`claude_desktop_config.json` or Claude Code settings)
2. Ensure `uvx` is installed and in PATH
3. Restart Claude Desktop/Code

---

### Symbol Editing Fails

**Cause**: Symbol body doesn't include signature, or name path is wrong.

**Fix**:
1. Use `find_symbol` with `include_body: true` to see expected format
2. Ensure `body` parameter includes the full definition (signature + implementation)

---

## When to Use Serena

### ✅ Use Serena When:

- **Exploring** a large, unfamiliar codebase
- **Refactoring** (renaming classes, methods, changing signatures)
- **Finding usages** of a function/class across the project
- **Understanding dependencies** (who calls this method?)
- **Targeted reads** (only read one method, not the whole file)
- **Adding code** to existing classes (insert methods)

---

### ❌ Don't Use Serena When:

- **No code exists yet** (planning/documentation phase)
- **Small projects** (< 10 files) where `Read` + `Edit` is faster
- **Non-code files** (markdown, JSON, YAML)
- **Greenfield scaffolding** (creating new files from scratch)
- **Simple find/replace** (use `Grep` + `Edit`)

---

## Serena in This Project (Flowingly Parsing Service)

### Current Status

- **Phase**: Specification and planning (no code yet)
- **Serena Status**: **Not yet usable** (no source files)
- **When to activate**: After **Milestone M0** (scaffold .NET solution + React app)

### Preparation Steps

**Now**:
1. Add `.serena/` to `.gitignore`
2. (Optional) Create `.serena/project.yml` with `language: csharp`

**After M0 Scaffold**:
1. Run `uvx --from git+https://github.com/oraios/serena serena project index`
2. Tell Claude: `Activate the project flowingly-technical-test`
3. Use Serena for semantic code navigation and editing

---

## Quick Reference

### Common Commands

```bash
# Index project
uvx --from git+https://github.com/oraios/serena serena project index

# Generate project config manually
uvx --from git+https://github.com/oraios/serena serena project generate-yml

# Check Serena version
uvx --from git+https://github.com/oraios/serena serena --version
```

### Common Claude Prompts

```
Activate the project /path/to/project
Get symbols overview for src/MyFile.cs
Find symbol "ClassName/MethodName" with include_body=true
Find all references to "MyClass"
Replace symbol body "ClassName/MethodName" with [new implementation]
List all memories
Write a memory "architecture-notes" with [content]
```

---

## Resources

- **GitHub**: https://github.com/oraios/serena
- **MCP Documentation**: https://modelcontextprotocol.io/
- **UV Installation**: https://docs.astral.sh/uv/

---

**Document Version**: 1.0
**Last Updated**: 2025-10-06
**Maintained By**: Development Team

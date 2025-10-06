#!/bin/bash
# scripts/reindex-serena.sh - Manually re-index Serena MCP
# Usage: ./scripts/reindex-serena.sh

set -e

echo "🔄 Re-indexing Serena MCP for semantic code analysis..."
echo ""

# Check if uvx is installed
if ! command -v uvx &> /dev/null; then
    echo "❌ Error: uvx not found"
    echo "   Install UV from https://docs.astral.sh/uv/"
    exit 1
fi

# Check if we're in the project root
if [[ ! -f "package.json" ]] && [[ ! -f "*.sln" ]]; then
    echo "❌ Error: Must run from project root"
    echo "   Current: $(pwd)"
    exit 1
fi

# Run Serena indexing
echo "📁 Project: $(pwd)"
echo "📊 Indexing C# codebase..."
echo ""

uvx --from git+https://github.com/oraios/serena serena project index

echo ""
echo "✅ Serena re-indexing complete!"
echo "   Index location: .serena/cache/"
echo ""
echo "💡 When to re-index:"
echo "   - After major code additions (new modules, classes)"
echo "   - After completing milestones (M0, M1, M2, M3)"
echo "   - If Serena tools return stale results"

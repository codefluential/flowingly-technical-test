#!/bin/bash
# scripts/clean.sh - Clean build artifacts

set -e

# Validate working directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

if [[ ! -f "$PROJECT_ROOT/package.json" ]] || [[ ! -d "$PROJECT_ROOT/src" ]]; then
  echo "❌ Error: Must run from project root or scripts directory"
  echo "   Current: $(pwd)"
  echo "   Expected: $PROJECT_ROOT"
  exit 1
fi

cd "$PROJECT_ROOT"

echo "🧹 Cleaning build artifacts..."
echo ""

echo "🔧 Cleaning backend..."
# Safe find - only within src/ directory, prevent directory modification during traversal
if [[ -d "src" ]]; then
  find src -type d \( -name "bin" -o -name "obj" \) -print0 2>/dev/null | while IFS= read -r -d '' dir; do
    echo "   Removing: $dir"
    rm -rf "$dir"
  done
fi

echo "⚛️  Cleaning frontend..."
if [[ -d "client/node_modules" ]]; then
  echo "   Removing: client/node_modules"
  rm -rf client/node_modules
fi

if [[ -d "client/dist" ]]; then
  echo "   Removing: client/dist"
  rm -rf client/dist
fi

echo ""
echo "✅ Clean complete!"

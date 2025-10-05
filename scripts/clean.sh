#!/bin/bash
# scripts/clean.sh - Clean build artifacts

set -e

echo "🧹 Cleaning build artifacts..."
echo ""

echo "🔧 Cleaning backend..."
find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true

echo "⚛️  Cleaning frontend..."
rm -rf client/node_modules client/dist 2>/dev/null || true

echo ""
echo "✅ Clean complete!"

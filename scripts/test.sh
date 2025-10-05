#!/bin/bash
# scripts/test.sh - Run all tests

set -e

echo "🧪 Running all tests..."
echo ""

echo "🔧 Backend Unit Tests..."
dotnet test --filter Category=Unit

echo ""
echo "🔧 Backend Contract Tests..."
dotnet test --filter Category=Contract

echo ""
echo "⚛️  Frontend E2E Tests..."
cd client
npm run test:e2e
cd ..

echo ""
echo "✅ All tests passed!"

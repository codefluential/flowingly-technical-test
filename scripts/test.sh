#!/bin/bash
# scripts/test.sh - Run all tests

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

echo "🧪 Running all tests..."
echo ""

# Track test results
TESTS_RUN=0
TESTS_PASSED=0

# Backend Unit Tests
echo "🔧 Backend Unit Tests..."
if dotnet test --list-tests --filter Category=Unit 2>/dev/null | grep -q "The following Tests are available"; then
  if dotnet test --filter Category=Unit --verbosity quiet; then
    ((TESTS_PASSED++))
  fi
  ((TESTS_RUN++))
else
  echo "   ⚠️  No unit tests found (expected during M0)"
fi

echo ""

# Backend Contract Tests
echo "🔧 Backend Contract Tests..."
if dotnet test --list-tests --filter Category=Contract 2>/dev/null | grep -q "The following Tests are available"; then
  if dotnet test --filter Category=Contract --verbosity quiet; then
    ((TESTS_PASSED++))
  fi
  ((TESTS_RUN++))
else
  echo "   ⚠️  No contract tests found (expected during M0-M1)"
fi

echo ""

# Frontend E2E Tests
echo "⚛️  Frontend E2E Tests..."
if [[ -f "client/package.json" ]] && grep -q "test:e2e" client/package.json; then
  cd client
  if npm run test:e2e; then
    ((TESTS_PASSED++))
  fi
  cd ..
  ((TESTS_RUN++))
else
  echo "   ⚠️  No E2E tests found (expected during M0-M2)"
fi

echo ""

# Summary
if [[ $TESTS_RUN -eq 0 ]]; then
  echo "⚠️  No tests found - this is expected during M0 (scaffold phase)"
  echo "   Tests will be added in M1 (TDD cycles) and M3 (E2E tests)"
  exit 0
elif [[ $TESTS_PASSED -eq $TESTS_RUN ]]; then
  echo "✅ All $TESTS_PASSED/$TESTS_RUN test suites passed!"
  exit 0
else
  echo "❌ Some tests failed: $TESTS_PASSED/$TESTS_RUN test suites passed"
  exit 1
fi

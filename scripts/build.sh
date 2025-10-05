#!/bin/bash
# scripts/build.sh - Production build

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

echo "📦 Building for production..."
echo ""

# Backend build
echo "🔧 Building backend..."
dotnet build --configuration Release

# Verify backend output
BACKEND_DLL=$(find . -path "*/bin/Release/net8.0/Api.dll" 2>/dev/null | head -n1)
if [[ -z "$BACKEND_DLL" ]]; then
  echo "❌ Backend build failed - Api.dll not found"
  exit 1
fi
echo "   ✅ Backend DLL: $BACKEND_DLL"

echo ""

# Frontend build
echo "⚛️  Building frontend..."
cd client
npm install
npm run build

# Verify frontend output
if [[ ! -d "dist" ]] || [[ ! -f "dist/index.html" ]]; then
  echo "❌ Frontend build failed - dist/index.html not found"
  exit 1
fi

DIST_SIZE=$(du -sh dist | cut -f1)
echo "   ✅ Frontend bundle: $DIST_SIZE"
cd ..

echo ""
echo "✅ Production build complete!"
echo "   Backend:  $BACKEND_DLL"
echo "   Frontend: client/dist/ ($DIST_SIZE)"

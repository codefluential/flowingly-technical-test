#!/bin/bash
# scripts/dev.sh - Start both dev servers

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

# Cleanup function
cleanup() {
  echo ""
  echo "🛑 Shutting down servers..."

  if [[ -n "$BACKEND_PID" ]] && kill -0 "$BACKEND_PID" 2>/dev/null; then
    echo "   Stopping backend (PID: $BACKEND_PID)..."
    kill -TERM "$BACKEND_PID" 2>/dev/null || true
  fi

  if [[ -n "$FRONTEND_PID" ]] && kill -0 "$FRONTEND_PID" 2>/dev/null; then
    echo "   Stopping frontend (PID: $FRONTEND_PID)..."
    kill -TERM "$FRONTEND_PID" 2>/dev/null || true
  fi

  # Wait for graceful shutdown
  sleep 2

  # Force kill if still running
  if [[ -n "$BACKEND_PID" ]] && kill -0 "$BACKEND_PID" 2>/dev/null; then
    kill -9 "$BACKEND_PID" 2>/dev/null || true
  fi

  if [[ -n "$FRONTEND_PID" ]] && kill -0 "$FRONTEND_PID" 2>/dev/null; then
    kill -9 "$FRONTEND_PID" 2>/dev/null || true
  fi

  echo "✅ Cleanup complete"
  exit 0
}

# Trap signals
trap cleanup SIGINT SIGTERM EXIT

echo "🚀 Starting Flowingly Parsing Service..."
echo ""

echo "📦 Installing frontend dependencies..."
cd client && npm install && cd ..

echo ""
echo "🔧 Starting backend API (port 5000)..."
dotnet run --project src/Api &
BACKEND_PID=$!

echo "⚛️  Starting frontend dev server (port 5173)..."
cd client && npm run dev &
FRONTEND_PID=$!

echo ""
echo "✅ Both servers started!"
echo "   Backend:  http://localhost:5000 (PID: $BACKEND_PID)"
echo "   Frontend: http://localhost:5173 (PID: $FRONTEND_PID)"
echo "   Swagger:  http://localhost:5000/swagger"
echo ""
echo "Press Ctrl+C to stop both servers"

# Wait for both processes
wait $BACKEND_PID $FRONTEND_PID

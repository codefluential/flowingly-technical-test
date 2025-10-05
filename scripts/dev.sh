#!/bin/bash
# scripts/dev.sh - Start both dev servers

set -e

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
echo "   Backend:  http://localhost:5000"
echo "   Frontend: http://localhost:5173"
echo "   Swagger:  http://localhost:5000/swagger"
echo ""
echo "Press Ctrl+C to stop both servers"

# Wait for both processes
wait $BACKEND_PID $FRONTEND_PID

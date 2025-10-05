#!/bin/bash
# Simple dashboard viewer script
# Starts a local web server and opens the dashboard in your browser

PORT=8080
DASHBOARD_URL="http://localhost:$PORT/dashboard/index.html"

# Check if server is already running
if lsof -Pi :$PORT -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "✅ Server already running on port $PORT"
else
    echo "🚀 Starting web server on port $PORT..."
    cd "$(dirname "$0")/.." && python3 -m http.server $PORT >/dev/null 2>&1 &
    sleep 1
fi

echo "📊 Dashboard URL: $DASHBOARD_URL"
echo ""
echo "Opening in browser..."

# Try different methods to open browser
if command -v wslview >/dev/null 2>&1; then
    wslview "$DASHBOARD_URL"
elif command -v xdg-open >/dev/null 2>&1; then
    xdg-open "$DASHBOARD_URL"
else
    echo "⚠️  Could not auto-open browser."
    echo "Please manually open: $DASHBOARD_URL"
fi

echo ""
echo "Press Ctrl+C to stop the server, or run:"
echo "  pkill -f 'python3 -m http.server $PORT'"

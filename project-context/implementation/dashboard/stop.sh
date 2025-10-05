#!/bin/bash
# Stop the dashboard web server

PORT=8080

if lsof -Pi :$PORT -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "🛑 Stopping web server on port $PORT..."
    pkill -f "python3 -m http.server $PORT"
    sleep 1

    # Verify it stopped
    if lsof -Pi :$PORT -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo "⚠️  Server still running. Trying force kill..."
        pkill -9 -f "python3 -m http.server $PORT"
    else
        echo "✅ Server stopped successfully"
    fi
else
    echo "ℹ️  No server running on port $PORT"
fi

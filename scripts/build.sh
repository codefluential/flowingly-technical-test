#!/bin/bash
# scripts/build.sh - Production build

set -e

echo "📦 Building for production..."
echo ""

echo "🔧 Building backend..."
dotnet build --configuration Release

echo ""
echo "⚛️  Building frontend..."
cd client
npm install
npm run build
cd ..

echo ""
echo "✅ Production build complete!"
echo "   Backend:  bin/Release/net8.0/"
echo "   Frontend: client/dist/"

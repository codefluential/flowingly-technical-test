# scripts/dev.ps1 - Windows PowerShell version

Write-Host "🚀 Starting Flowingly Parsing Service..." -ForegroundColor Green
Write-Host ""

Write-Host "📦 Installing frontend dependencies..." -ForegroundColor Cyan
Set-Location client
npm install
Set-Location ..

Write-Host ""
Write-Host "🔧 Starting backend API (port 5000)..." -ForegroundColor Cyan
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run","--project","src/Api"

Start-Sleep -Seconds 2

Write-Host "⚛️  Starting frontend dev server (port 5173)..." -ForegroundColor Cyan
Set-Location client
Start-Process -NoNewWindow -FilePath "npm" -ArgumentList "run","dev"
Set-Location ..

Write-Host ""
Write-Host "✅ Both servers started!" -ForegroundColor Green
Write-Host "   Backend:  http://localhost:5000"
Write-Host "   Frontend: http://localhost:5173"
Write-Host "   Swagger:  http://localhost:5000/swagger"
Write-Host ""
Write-Host "Press Ctrl+C to stop servers" -ForegroundColor Yellow

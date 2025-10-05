# scripts/dev.ps1 - Windows PowerShell version

$ErrorActionPreference = "Stop"

# Validate working directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

if (-not (Test-Path "$ProjectRoot/package.json") -or -not (Test-Path "$ProjectRoot/src")) {
    Write-Host "❌ Error: Must run from project root or scripts directory" -ForegroundColor Red
    Write-Host "   Current: $(Get-Location)"
    Write-Host "   Expected: $ProjectRoot"
    exit 1
}

Set-Location $ProjectRoot

# Process tracking
$Script:BackendProcess = $null
$Script:FrontendProcess = $null

# Cleanup function
function Cleanup {
    Write-Host ""
    Write-Host "🛑 Shutting down servers..." -ForegroundColor Yellow

    if ($Script:BackendProcess -and -not $Script:BackendProcess.HasExited) {
        Write-Host "   Stopping backend (PID: $($Script:BackendProcess.Id))..." -ForegroundColor Cyan
        Stop-Process -Id $Script:BackendProcess.Id -Force -ErrorAction SilentlyContinue
    }

    if ($Script:FrontendProcess -and -not $Script:FrontendProcess.HasExited) {
        Write-Host "   Stopping frontend (PID: $($Script:FrontendProcess.Id))..." -ForegroundColor Cyan
        Stop-Process -Id $Script:FrontendProcess.Id -Force -ErrorAction SilentlyContinue
    }

    # Kill any orphaned dotnet/node processes on dev ports
    Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | ForEach-Object {
        Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
    }

    Get-NetTCPConnection -LocalPort 5173 -ErrorAction SilentlyContinue | ForEach-Object {
        Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
    }

    Write-Host "✅ Cleanup complete" -ForegroundColor Green
}

# Register cleanup on exit
Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { Cleanup } | Out-Null

# Trap Ctrl+C
$null = [System.Console]::TreatControlCAsInput = $false
try {
    Write-Host "🚀 Starting Flowingly Parsing Service..." -ForegroundColor Green
    Write-Host ""

    Write-Host "📦 Installing frontend dependencies..." -ForegroundColor Cyan
    Set-Location client
    npm install
    if ($LASTEXITCODE -ne 0) { throw "npm install failed" }
    Set-Location ..

    Write-Host ""
    Write-Host "🔧 Starting backend API (port 5000)..." -ForegroundColor Cyan
    $Script:BackendProcess = Start-Process -NoNewWindow -PassThru -FilePath "dotnet" -ArgumentList "run","--project","src/Api"

    Start-Sleep -Seconds 2

    Write-Host "⚛️  Starting frontend dev server (port 5173)..." -ForegroundColor Cyan
    Set-Location client
    $Script:FrontendProcess = Start-Process -NoNewWindow -PassThru -FilePath "npm" -ArgumentList "run","dev"
    Set-Location ..

    Write-Host ""
    Write-Host "✅ Both servers started!" -ForegroundColor Green
    Write-Host "   Backend:  http://localhost:5000 (PID: $($Script:BackendProcess.Id))"
    Write-Host "   Frontend: http://localhost:5173 (PID: $($Script:FrontendProcess.Id))"
    Write-Host "   Swagger:  http://localhost:5000/swagger"
    Write-Host ""
    Write-Host "Press Ctrl+C to stop servers" -ForegroundColor Yellow

    # Wait for user interrupt
    while ($true) {
        Start-Sleep -Seconds 1

        # Check if processes are still running
        if ($Script:BackendProcess.HasExited -or $Script:FrontendProcess.HasExited) {
            Write-Host ""
            Write-Host "⚠️  One or more servers stopped unexpectedly" -ForegroundColor Yellow
            break
        }
    }
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
}
finally {
    Cleanup
}

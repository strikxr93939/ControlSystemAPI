# Start all components of VersionControl System
# Run from the repo root as Administrator (for process kill permissions)

param(
    [switch]$ApiOnly,
    [switch]$NoFrontend
)

$root = $PSScriptRoot

function Start-InNewWindow([string]$Title, [string]$WorkDir, [string]$Command) {
    Start-Process powershell -ArgumentList `
        "-NoExit", "-Command", `
        "cd '$WorkDir'; Write-Host '=== $Title ===' -ForegroundColor Cyan; $Command" `
        -WindowStyle Normal
}

# 1. Start API
Start-InNewWindow "VersionControl API :5186" `
    "$root\backend\VersionControl.Api" `
    "dotnet run --launch-profile http"

Start-Sleep -Seconds 3

if (-not $ApiOnly) {
    # 2. Start Agent
    Start-InNewWindow "VersionControl Agent" `
        "$root\agent\versioncontrol.agent" `
        "dotnet run"

    if (-not $NoFrontend) {
        # 3. Start Admin Panel
        Start-InNewWindow "Admin Panel :3000" `
            "$root\admin-client" `
            "npm install; npm start"
    }
}

Write-Host ""
Write-Host "Started! Endpoints:" -ForegroundColor Green
Write-Host "  API:     http://localhost:5186"      -ForegroundColor White
Write-Host "  Swagger: http://localhost:5186/swagger" -ForegroundColor White
Write-Host "  Admin:   http://localhost:3000"      -ForegroundColor White
Write-Host ""
Write-Host "To seed test policies:" -ForegroundColor Yellow
Write-Host "  .\scripts\seed-policies.ps1"

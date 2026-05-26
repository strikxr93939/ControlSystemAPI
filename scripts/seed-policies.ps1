# Seed test policies — run from Windows with API running
param([string]$ApiUrl = "http://localhost:5186")

$headers = @{ "Content-Type" = "application/json" }

# Policy 1: Notepad version warning
$body1 = @{
    programPattern = "notepad"
    minVersion     = "99.0.0.0"
    blockType      = 0
    workshop       = "Workshop A"
    message        = "Notepad version is outdated"
    isActive       = $true
    startTime      = "2026-01-01T00:00:00Z"
    exceptions     = ""
} | ConvertTo-Json

$r1 = Invoke-RestMethod -Method POST -Uri "$ApiUrl/api/policies" -Headers $headers -Body $body1
Write-Host "Created policy: $($r1.id) — $($r1.programPattern)"

# Policy 2: Block torrent software
$body2 = @{
    programPattern = "torrent"
    blockType      = 2
    workshop       = ""
    message        = "Torrent software is forbidden"
    isActive       = $true
    startTime      = "2026-01-01T00:00:00Z"
    exceptions     = ""
} | ConvertTo-Json

$r2 = Invoke-RestMethod -Method POST -Uri "$ApiUrl/api/policies" -Headers $headers -Body $body2
Write-Host "Created policy: $($r2.id) — $($r2.programPattern)"

# Policy 3: Chrome minimum version
$body3 = @{
    programPattern = "chrome"
    minVersion     = "120.0.0.0"
    blockType      = 1
    workshop       = "Office"
    message        = "Chrome must be at least version 120"
    isActive       = $true
    startTime      = "2026-01-01T00:00:00Z"
    exceptions     = "DEVPC-001,DEVPC-002"
} | ConvertTo-Json

$r3 = Invoke-RestMethod -Method POST -Uri "$ApiUrl/api/policies" -Headers $headers -Body $body3
Write-Host "Created policy: $($r3.id) — $($r3.programPattern)"

Write-Host "`nAll policies created. Check: $ApiUrl/api/policies"

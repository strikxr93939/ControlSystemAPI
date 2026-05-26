# Run as Administrator
# Installs VersionControl.Agent as a Windows Service

param(
    [string]$ApiUrl     = "http://localhost:5186/",
    [string]$PublishDir = "$PSScriptRoot\agent-published"
)

Write-Host "Publishing Agent..."
Push-Location "$PSScriptRoot\..\agent\versioncontrol.agent"
dotnet publish -c Release -r win-x64 --self-contained -o $PublishDir
Pop-Location

$exePath = Join-Path $PublishDir "versioncontrol.agent.exe"

Write-Host "Installing Windows Service..."
New-Service `
    -Name       "VersionControlAgent" `
    -BinaryPathName $exePath `
    -DisplayName "VersionControl Agent" `
    -Description "Monitors running processes and enforces software version policies" `
    -StartupType Automatic

# Set Agent:ApiBaseUrl environment for the service
[System.Environment]::SetEnvironmentVariable(
    "Agent__ApiBaseUrl", $ApiUrl,
    [System.EnvironmentVariableTarget]::Machine)

Start-Service "VersionControlAgent"
Write-Host "Service started. Status:"
Get-Service "VersionControlAgent"

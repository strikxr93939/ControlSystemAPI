@echo off
cd /d "%~dp0..\backend\VersionControl.Api"
echo Starting VersionControl API on http://localhost:5186 ...
dotnet run --launch-profile http

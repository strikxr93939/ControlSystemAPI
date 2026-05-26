@echo off
cd /d "%~dp0..\agent\versioncontrol.agent"
echo Starting VersionControl Agent ...
dotnet run

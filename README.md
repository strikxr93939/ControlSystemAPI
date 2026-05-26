# VersionControl System — Software Monitoring & Enforcement

Production-ready system for monitoring software versions and enforcing policies across computers.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│  Admin Panel (React + SignalR)   :3000                  │
│  Real-time violations dashboard                         │
└─────────────────────────┬───────────────────────────────┘
                          │ HTTP + WebSocket (SignalR)
┌─────────────────────────▼───────────────────────────────┐
│  ASP.NET Core Web API            :5186                  │
│  • GET/POST /api/violations                             │
│  • GET/POST/PUT/DELETE /api/policies                    │
│  • GET/POST/PUT/DELETE /api/computers                   │
│  • SignalR Hub /hubs/monitoring                         │
│  • Swagger UI /swagger                                  │
└──────────┬───────────────────────┬──────────────────────┘
           │ SQLite (app.db)       │ SignalR push
┌──────────▼──────────┐   ┌───────▼──────────────────────┐
│  SQLite Database    │   │  Agent (Windows Service)      │
│  • Violations       │   │  • Scans processes every 10s  │
│  • Policies         │   │  • Checks against policies    │
│  • Computers        │   │  • Version comparison         │
│  • Programs         │   │  • Sends violations to API    │
└─────────────────────┘   └──────────────────────────────┘
```

## Tech Stack

- **Backend**: ASP.NET Core 10, Entity Framework Core 10, SQLite
- **Realtime**: SignalR
- **Agent**: .NET Worker Service
- **Frontend**: React 18, @microsoft/signalr

## Quick Start

### 1. Start the API

```bash
cd backend/VersionControl.Api
dotnet run
# API running at http://localhost:5186
# Swagger at http://localhost:5186/swagger
```

### 2. Start the Agent (Windows only for process scanning)

```bash
cd agent/versioncontrol.agent
dotnet run
# Agent polls API every 10 seconds
```

### 3. Start Admin Panel

```bash
cd admin-client
npm install
npm start
# Dashboard at http://localhost:3000
```

## Testing End-to-End

### 1. Create a test policy via Swagger (http://localhost:5186/swagger):

```
POST /api/policies
{
  "programPattern": "notepad",
  "minVersion": "99.0.0.0",
  "blockType": 0,
  "workshop": "Test",
  "message": "Notepad version too old",
  "isActive": true,
  "startTime": "2026-01-01T00:00:00Z"
}
```

### 2. Open Notepad on the monitored machine.

### 3. Watch the Admin Panel — a violation appears within 10 seconds via SignalR.

### 4. Check violations:

```
GET /api/violations
```

## API Reference

### Violations

| Method | Path                     | Description                   |
|--------|--------------------------|-------------------------------|
| GET    | /api/violations          | Get last 100 violations       |
| GET    | /api/violations/{id}     | Get single violation          |
| POST   | /api/violations          | Create violation (Agent)      |
| DELETE | /api/violations/{id}     | Delete violation              |

### Policies

| Method | Path                         | Description                |
|--------|------------------------------|----------------------------|
| GET    | /api/policies                | Get all policies           |
| GET    | /api/policies?activeOnly=true| Get active only            |
| POST   | /api/policies                | Create policy              |
| PUT    | /api/policies/{id}           | Update policy              |
| PATCH  | /api/policies/{id}/toggle    | Toggle active/inactive     |
| DELETE | /api/policies/{id}           | Delete policy              |

### SignalR Events (hub: /hubs/monitoring)

| Event                  | When fired                        |
|------------------------|-----------------------------------|
| ViolationReceived      | New violation saved               |
| PoliciesUpdated        | Policy created/updated/deleted    |
| ComputerStatusChanged  | Computer status changed           |

## Policy BlockType Values

| Value | Name      | Effect                                      |
|-------|-----------|---------------------------------------------|
| 0     | Warning   | Log and report only                         |
| 1     | SoftBlock | Report + notify user                        |
| 2     | HardBlock | Report + kill the process                   |
| 3     | Timed     | Report within time window                   |

## Project Structure

```
VersionControlSystem/
├── backend/
│   ├── VersionControl.Api/         # Web API + Controllers + Hubs
│   │   ├── Controllers/
│   │   │   ├── ViolationsController.cs
│   │   │   ├── PoliciesController.cs
│   │   │   └── ComputersController.cs
│   │   ├── DTOs/
│   │   │   ├── ViolationDto.cs
│   │   │   └── PolicyDto.cs
│   │   ├── Hubs/
│   │   │   └── MonitoringHub.cs
│   │   └── Program.cs
│   ├── VersionControl.Application/ # Interfaces / Use cases
│   ├── VersionControl.Domain/      # Entities + Enums
│   ├── VersionControl.Infrastructure/ # EF Core + Migrations
│   └── VersionControl.Shared/      # Shared utilities
├── agent/
│   └── versioncontrol.agent/
│       ├── Services/
│       │   ├── ApiService.cs       # HTTP client → API
│       │   ├── PolicyEvaluator.cs  # Version comparison logic
│       │   ├── ProcessScanner.cs   # Process enumeration
│       │   └── VersionDetector.cs  # FileVersionInfo reader
│       ├── Models/
│       │   ├── PolicyDto.cs
│       │   └── ViolationRequest.cs
│       ├── Worker.cs               # BackgroundService main loop
│       └── Program.cs
└── admin-client/                   # React dashboard
    └── src/
        ├── App.js                  # Main component (tabs)
        ├── api.js                  # Fetch wrappers
        ├── signalr.js              # SignalR connection
        └── index.js
```

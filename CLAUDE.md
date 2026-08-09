# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

WaterLevelAPI is a minimal ASP.NET Core 8 Web API that receives water-level telemetry from IoT devices and stores it in SQLite via EF Core. It is the backend for a separate PWA client project (`WaterLevelPWA`, sibling directory, own git repo) which polls/consumes this API.

The actual .NET project lives in the nested `WaterLevelAPI/` subfolder (repo root -> `WaterLevelAPI.slnx` -> `WaterLevelAPI/WaterLevelAPI.csproj`).

## Commands

Run from the `WaterLevelAPI/` project folder (where `WaterLevelAPI.csproj` lives):

```
dotnet restore
dotnet build
dotnet run
```

There are no test projects in the solution currently.

EF Core migrations (Sqlite provider):
```
dotnet ef migrations add <Name>
dotnet ef database update
```

Manual HTTP requests can be made via `WaterLevelAPI.http` (uses `GET /weatherforecast/` as a placeholder — the real endpoints are under `/api/WaterLevel`, see below).

## Architecture

Standard 3-layer flow: Controller -> Service (interface + impl) -> EF Core DbContext -> SQLite.

- `Program.cs` — composition root. Registers `IWaterLevelService`/`WaterLevelService` as scoped, `AddSignalR()`, and `AppDbContext` with the Sqlite connection string from `ConnectionStrings:DefaultConnection` (`appsettings.json`). Calls `dbContext.Database.EnsureCreated()` at startup instead of applying migrations programmatically — the `Migrations/` folder exists but is not auto-applied on boot. Swagger is only wired up in the Development environment.
- `Controllers/WaterLevelController.cs` — exposes `api/WaterLevel` with two actions: `POST` (`RegisterLevel`, named `telemetry`) to ingest a reading, `GET` (`GetCurrentLevel`, named `current`) to fetch the latest reading for a `deviceId` query param. Both return `Accepted` (202) rather than typical 200/201 — this is intentional per existing convention, keep it consistent for new endpoints. Domain validation failures surface as `ArgumentException` and are translated to 400; anything else becomes 500 with the exception message in the body (no generic error wrapping).
- `Service/IWaterLevelService.cs` / `Service/WaterLevelService.cs` — business logic. `RegisterLevelAsync` currently only rejects negative `CurrentLevel`; the min/max-bounds check is present but commented out in `WaterLevelService.cs` — don't silently re-enable it without confirming with the user, since it was deliberately disabled. `GetLevelAsync` returns the most recent reading for a device ordered by `TimesTamp` descending, throwing `ArgumentException` (-> 400) when none exists rather than returning null/404.
- `Model/WatterLevel.cs` — EF entity (note the "Watter" typo in the type name and filename — this is intentional/existing, not a typo to silently "fix" since it's already baked into migrations and the DB schema).
- `WaterLevelDTO.cs` — request/response DTO, deliberately kept in the project root namespace (`WaterLevelAPI`, not `WaterLevelAPI.DTO` or similar).
- `AppDbContext` exposes a single `DbSet<WatterLevel> WaterLevels`.
- SignalR is registered in DI (`AddSignalR()`) but no hub class exists yet and no hub is mapped in `Program.cs` — treat this as scaffolding for a future real-time push feature (e.g. pushing new readings to the PWA), not a currently working feature.

## Data

`WaterLevel.db` is a real Sqlite file checked into the repo (used as the dev/runtime database, not just a fixture). It changes as the app runs — expect it to show up as modified in `git status` after local testing; don't assume unrelated diffs on it indicate a bug.

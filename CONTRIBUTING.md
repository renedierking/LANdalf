# Contributing

Thanks for taking the time to contribute!

## Quick Start

- Install the .NET SDK referenced in the README.
- Build: `dotnet build LANdalf.slnx`
- Test: `dotnet test`
- Run API: `dotnet run --project src/API/API.csproj`
- Run UI: `dotnet run --project src/UI/UI.csproj`

## Development Guidelines

- Keep changes focused and small when possible.
- Add/adjust tests for behavior changes.
- Prefer simple, readable code over clever solutions.
- Follow the repository formatting rules (see `.editorconfig`).

## Pull Requests

A good PR includes:
- What/why summary
- Test coverage notes (`dotnet test`)
- Any docs updates needed (README, comments, etc.)

## Reporting Bugs / Requesting Features

Please open an issue with:
- Expected vs actual behavior
- Reproduction steps
- Logs/screenshots if helpful
- Environment details (OS, Docker vs local, browser for UI)

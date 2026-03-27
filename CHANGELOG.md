# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Minimal API Strategy Pattern** — Endpoint registration refactored into strategy classes (`IMinimalApiStrategy`, `MinimalApiStrategyExtensions` with `AddMinimalApiStrategies()` assembly scanning and `MapMinimalApiStrategies()`). `PcDeviceMinimalApiStrategy` moves all `/pc-devices/` endpoint definitions out of `Program.cs`.
- **Card View for Devices** — Home page now supports toggling between a table view and a card view via `DeviceCard.razor` and `DeviceEditDialog.razor` components.
- **`ViewPreferenceService`** — Persists the card/table view preference to `localStorage`; registered in `Program.cs` (UI).
- **Footer Bar** — A persistent bottom `MudAppBar` showing the app version and a GitHub link.
- **Snackbar Notifications** — Add, update, delete, and WoL actions now show `ISnackbar` toasts.
- **`PcDeviceDTO.Clone()` Extension** — New `Extensions.cs` in `src/UI/ApiClient/` adds a `Clone()` helper to avoid editing live objects in dialogs.
- **Development `appsettings`** — Added `src/UI/wwwroot/appsettings.development.json` with `ApiBaseAddress` set to `https://localhost:7206/` for local development; `Program.cs` (UI) uses `#if DEBUG` to select the right base address.
- `MinimalApiStrategyTests` — New tests covering `AddMinimalApiStrategies` registration, deduplication, and `MapMinimalApiStrategies` delegation.

### Changed

- `Home.razor` fully refactored: inline data-grid edit replaced by a dedicated `DeviceEditDialog` modal; `CommittedItemChanges` removed; separate `AddNewDevice`/`UpdateDevice` methods introduced; `LoadData` split into initial load + `RefreshData`.
- `MainLayout.razor`: theme toggle button uses `aria-label` instead of `Title`; footer `MudAppBar` added.
- `ThemeService.cs`: dark-mode primary color changed from `#bb86fc` to `#4fc3f7`.
- `Program.cs` (API): all endpoint definitions moved to `PcDeviceMinimalApiStrategy`.
- `HomeComponentTests` updated to inject `ViewPreferenceService`.
- `MudBlazor` upgraded from `8.*` to `9.2.0`.
- `dotnet-ef`: `10.0.1` → `10.0.5`
- `Microsoft.AspNetCore.OpenApi`: `10.0.3` → `10.0.5`
- `Microsoft.EntityFrameworkCore.Sqlite`: `10.0.3` → `10.0.5`
- `Microsoft.EntityFrameworkCore.Tools`: `10.0.3` → `10.0.5`
- `Scalar.AspNetCore`: `2.12.50` → `2.13.15`
- `FluentAssertions`: `8.8.0` → `8.9.0`
- `Microsoft.EntityFrameworkCore.InMemory`: `10.0.3` → `10.0.5`
- `Microsoft.AspNetCore.Mvc.Testing`: `10.0.3` → `10.0.5`
- GitHub Actions: `docker/setup-qemu-action` v3→v4, `docker/setup-buildx-action` v3→v4, `docker/login-action` v3→v4, `docker/metadata-action` v5→v6, `docker/build-push-action` v6→v7

## [1.0.1]

### Added

- Initial public release
- Wake-on-LAN device management with RESTful API
- Blazor WebAssembly frontend with MudBlazor UI
- Docker Compose deployment support
- SQLite database for persistent storage
- Real-time device status monitoring
- OpenAPI/Swagger API documentation

[Unreleased]: https://github.com/renedierking/LANdalf/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/renedierking/LANdalf/compare/v1.0.0...v1.0.1

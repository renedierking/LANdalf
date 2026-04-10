# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Device Groups** — Organize devices into named categories (e.g., "Gaming", "Work", "Media Server"). New nullable `GroupName` field on `PcDevice` model enables lightweight categorization. Card view displays devices grouped by category with "Ungrouped" section for unassigned devices (sorted last). Table view includes filterable `GroupName` column. Group badge (MudChip) shown in `DeviceCard` header.
- **Enhanced Loading Screen** — Redesigned Blazor WASM initial loading page with modern dark gradient background (`#1a1a2e` → `#0f3460`), animated LANdalf wizard emoji (🧙‍♂️) with floating animation, cyan progress circle with glow effect matching app theme, and centered "LANdalf" app name display.
- **Device Monitoring Service** — Background `IHostedService` that automatically pings devices at configurable intervals (default 30s) to track online/offline status. Updates `IsOnline` field in database and broadcasts changes via SignalR for real-time UI updates.
- **Real-time Status Updates** — SignalR `DeviceStatusHub` broadcasts device status changes to connected clients. `DeviceStatusHubService` (UI) manages WebSocket connections with automatic reconnection.
- **Online Timestamp Tracking** — `OnlineSince` field tracks when devices come online (UTC). UI displays user-friendly durations ("5 minutes ago", "2 hours ago") via `Formatter.FormatOnlineSince()`.
- **Device Monitoring Configuration** — `DeviceMonitoringOptions` class using IOptions pattern for type-safe settings (`Enabled`, `IntervalSeconds`, `TimeoutMilliseconds`) in `appsettings.json`.
- **Docker Ping Utilities** — Added `iputils-ping` package installation in API Dockerfile for ICMP ping support in containerized environments. Enhanced error handling for `PlatformNotSupportedException` with thread-safe logging guard.
- **Minimal API Strategy Pattern** — Endpoint registration refactored into strategy classes (`IMinimalApiStrategy`, `MinimalApiStrategyExtensions` with `AddMinimalApiStrategies()` assembly scanning and `MapMinimalApiStrategies()`). `PcDeviceMinimalApiStrategy` moves all `/pc-devices/` endpoint definitions out of `Program.cs`.
- **Card View for Devices** — Home page now supports toggling between a table view and a card view via `DeviceCard.razor` and `DeviceEditDialog.razor` components.
- **`ViewPreferenceService`** — Persists the card/table view preference to `localStorage`; registered in `Program.cs` (UI).
- **Footer Bar** — A persistent bottom `MudAppBar` showing the app version and a GitHub link.
- **Snackbar Notifications** — Add, update, delete, and WoL actions now show `ISnackbar` toasts.
- **`PcDeviceDTO.Clone()` Extension** — New `Extensions.cs` in `src/UI/ApiClient/` adds a `Clone()` helper to avoid editing live objects in dialogs.
- **Development `appsettings`** — Added `src/UI/wwwroot/appsettings.development.json` with `ApiBaseAddress` set to `https://localhost:7206/` for local development; `Program.cs` (UI) uses `#if DEBUG` to select the right base address.
- `MinimalApiStrategyTests` — New tests covering `AddMinimalApiStrategies` registration, deduplication, and `MapMinimalApiStrategies` delegation.
- `DeviceMonitoringServiceTests` — 16 new tests covering configuration validation, ping logic, status transitions, parallel processing, SignalR broadcasting, and IOptions pattern.
- `DtoExtensionsTests` — New tests covering `GroupName` field mapping in device DTO conversions.

### Changed

- **Database Schema** — Added `GroupName` (nullable string) field with migration `20260329184944_AddDeviceGroupName`. Added `OnlineSince` (nullable DateTime) field with migration `20260328142753_AddOnlineSinceField`.
- **API Program.cs** — Registered `DeviceMonitoringService` as singleton + hosted service, configured `DeviceMonitoringOptions` from `appsettings.json`, added SignalR with `DeviceStatusHub` mapping.
- **UI Program.cs** — Registered `DeviceStatusHubService` as singleton, starts SignalR connection during app initialization.
- **DTOs** — Updated `PcDeviceDTO` and `PcCreateDto` to include `GroupName` field. Updated `DtoExtensions` mapping methods to handle device groups.
- `Home.razor` fully refactored: inline data-grid edit replaced by a dedicated `DeviceEditDialog` modal; `CommittedItemChanges` removed; separate `AddNewDevice`/`UpdateDevice` methods introduced; `LoadData` split into initial load + `RefreshData`. Now subscribes to `DeviceStatusHubService.OnDeviceStatusChanged` for real-time updates. Card view groups devices by `GroupName` with special handling for ungrouped devices.
- `DeviceCard.razor` displays group badge (MudChip) in header when device has a group assigned. Shows "online since" timestamp when device is online using `Formatter.FormatOnlineSince()`.
- `DeviceEditDialog.razor` added group input field with helper text for organizing devices into categories.
- `PcDeviceHandler` updated to persist `GroupName` on device create and update operations.
- `MainLayout.razor`: theme toggle button uses `aria-label` instead of `Title`; footer `MudAppBar` added.
- `ThemeService.cs`: dark-mode primary color changed from `#bb86fc` to `#4fc3f7`.
- `Program.cs` (API): all endpoint definitions moved to `PcDeviceMinimalApiStrategy`.
- `HomeComponentTests` updated to inject `ViewPreferenceService` and mock `DeviceStatusHubService`.
- `PcDeviceHandlerTests` updated to handle new `OnlineSince` and `GroupName` fields.
- `MudBlazor` upgraded from `8.*` to `9.3.0`.
- `bunit`: `2.6.2` → `2.7.2`
- `dotnet-ef`: `10.0.1` → `10.0.5`
- `Microsoft.AspNetCore.OpenApi`: `10.0.3` → `10.0.5`
- `Microsoft.EntityFrameworkCore.Sqlite`: `10.0.3` → `10.0.5`
- `Microsoft.EntityFrameworkCore.Tools`: `10.0.3` → `10.0.5`
- `Scalar.AspNetCore`: `2.12.50` → `2.13.20`
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

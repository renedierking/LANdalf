# Development Guide

## Build and Test

```bash
dotnet build LANdalf.slnx   # Build
dotnet test                  # Test
docker compose build         # Docker images
```

---

## Minimal API extension pattern

LANdalf uses a strategy pattern for Minimal API endpoint registration:

- Implement `IMinimalApiStrategy` in the API project.
- Register strategies through `AddMinimalApiStrategies()` (assembly scanning via `TryAddEnumerable` for idempotent registration).
- Strategies are applied in `Program.cs` via `MapMinimalApiStrategies(...)`.

This keeps `Program.cs` focused on composition and makes new endpoint groups plug-in friendly.

---

## View preference persistence

The `ViewPreferenceService` (in `src/UI/Services/`) stores the user's chosen view mode (card or table) in the browser's `localStorage` under the key `"view-preference"`. It is registered as a scoped service in `Program.cs` (UI) and injected into the `Home` page component.

---

## Device monitoring configuration

The `DeviceMonitoringService` (in `src/API/Services/`) runs as a background `IHostedService` that automatically pings devices to track their online/offline status. Configuration is managed through the IOptions pattern in `appsettings.json`:

```json
"DeviceMonitoring": {
  "Enabled": true,
  "IntervalSeconds": 30,
  "TimeoutMilliseconds": 2000
}
```

- **Enabled**: Toggle device monitoring on/off
- **IntervalSeconds**: How often to ping devices (minimum 5 seconds)
- **TimeoutMilliseconds**: Ping timeout threshold (minimum 100ms)

Status changes are broadcast to connected clients via SignalR's `DeviceStatusHub`, enabling real-time UI updates without polling.

---

See [CONTRIBUTING.md](../CONTRIBUTING.md) for prerequisites, project structure, and development guidelines.

# LANdalf Architecture

## Overview

LANdalf is a full-stack web application built with .NET 10.0, using a REST API backend with a Blazor WebAssembly frontend. The application is containerized with Docker for easy deployment.

```
┌─────────────────────────────────────────────────────────┐
│                   Client Browser                         │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Blazor WebAssembly Frontend (WASM)             │   │
│  │  • MudBlazor Components                         │   │
│  │  • State Management                             │   │
│  │  • Service Layer (API Client)                   │   │
│  └──────────────────────────────────────────────────┘   │
│                         ↑ HTTPS                          │
│                         ↓                                │
├─────────────────────────────────────────────────────────┤
│                  ASP.NET Core 10.0                       │
│                                                           │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Minimal API Endpoints (/api/v1/...)           │   │
│  │  • Device Management (CRUD)                    │   │
│  │  • WoL Operations                              │   │
│  │  • OpenAPI/Swagger Documentation               │   │
│  └──────────────────────────────────────────────────┘   │
│                         ↓                                │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Service Layer                                   │   │
│  │  • WakeOnLanService (WoL magic packets)        │   │
│  │  • PcDeviceHandler (Business logic)            │   │
│  │  • AppDbService (Data access)                  │   │
│  └──────────────────────────────────────────────────┘   │
│                         ↓                                │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Data Layer (Entity Framework Core)            │   │
│  │  • AppDbContext                                │   │
│  │  • Models & DTOs                               │   │
│  │  • Database Migrations                         │   │
│  └──────────────────────────────────────────────────┘   │
│                         ↓                                │
├─────────────────────────────────────────────────────────┤
│                   SQLite Database                        │
│          (LANdalf_Data/landalf.db)                       │
└─────────────────────────────────────────────────────────┘
```

---

## Project Structure

### Source Code (`/src`)

#### **API Project** (`/src/API`)
The ASP.NET Core Web API providing RESTful endpoints for device management and WoL operations.

- **Program.cs**: Application startup, service registration, middleware configuration
- **Data/**: Database context and configuration
  - `AppDbContext.cs`: EF Core DbContext
- **Models/**: Domain entities
  - `PcDevice.cs`: Device model
- **DTOs/**: Data Transfer Objects for API contracts
  - `PcDeviceDTO.cs`: Device API contract
  - `PcCreateDto.cs`: Device creation contract
- **Services/**: Business logic
  - `IAppDbService.cs`, `AppDbService.cs`: Data access layer
  - `WakeOnLanService.cs`: WoL magic packet generation and sending
- **Handler/**: Request handlers
  - `PcDeviceHandler.cs`: Device business logic
- **Extensions/**: Utility extensions
  - `DtoExtensions.cs`: DTO mapping helpers
- **Migrations/**: EF Core database migrations
  - Auto-generated from model changes

#### **UI Project** (`/src/UI`)
The Blazor WebAssembly application providing the user interface.

- **Program.cs**: WASM host configuration, service registration
- **App.razor**: Root component
- **Pages/**: Routable components
  - `Home.razor`: Main device management page
  - `NotFound.razor`: 404 page
- **Components/**: Reusable components
  - `CancellationTokenComponentBase.cs`: Base class for cancellation support
- **Layout/**: Application layout
  - `MainLayout.razor`: Main layout structure
- **Services/**: Client-side services
  - `ILANdalfApiService.cs`, `LANdalfApiService.cs`: API client wrapper
  - `ThemeService.cs`: Theme management (dark/light mode)
- **ApiClient/**: Auto-generated API client
  - `LANdalfApiClient.cs`: Generated by NSwag from OpenAPI spec
  - `LANdalfApiClientProxy.cs`: Proxy for partial class customization
  - `apiclient.nswag`: NSwag configuration file

#### **Test Projects** (`/test`)
- **API.Tests**: API unit tests
- **UI.Tests**: UI component tests using Bunit

---

## Key Technologies

### Backend
- **Framework**: ASP.NET Core 10.0
- **Database**: Entity Framework Core with SQLite
- **API Documentation**: OpenAPI/Swagger with Scalar
- **API Versioning**: Asp.Versioning v8+

### Frontend
- **Framework**: Blazor WebAssembly (WASM)
- **UI Library**: MudBlazor (Material Design)
- **HTTP Client**: NSwag-generated client
- **Theme Management**: Custom CSS with theme variables

### DevOps
- **Containerization**: Docker & Docker Compose
- **Build**: .NET CLI (dotnet build/publish)
- **Testing**: xUnit & Bunit
- **CI/CD**: GitHub Actions

---

## Data Flow

### Device Wake Flow
```
1. User clicks "Wake" button
   ↓
2. UI calls LANdalfApiService.WakePcDeviceAsync(deviceId)
   ↓
3. API endpoint: POST /api/v1/pc-devices/{id}/wake
   ↓
4. PcDeviceHandler retrieves device from database
   ↓
5. WakeOnLanService.Wake() is called with:
   - MAC address
   - Broadcast address (or auto-detected)
   ↓
6. Magic packet is generated and sent via UDP:
   - Port 7 or 9 (configurable)
   - Broadcast or unicast (configurable)
   ↓
7. Response returned to UI
   ↓
8. UI updates status
```

### Device Management Flow
```
GET /api/v1/pc-devices
   ↓
API endpoint retrieves all devices
   ↓
AppDbService queries database
   ↓
Results mapped to DTOs
   ↓
Returned as JSON to UI
   ↓
UI renders device list
```

---

## Database Schema

### PcDevice Table
```sql
CREATE TABLE PcDevices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    MacAddress TEXT NOT NULL UNIQUE,
    IpAddress TEXT,
    BroadcastAddress TEXT,
    Description TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastModified DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

---

## Configuration

### Environment Variables

#### API
- `Cors:FrontendUrl`: CORS policy URL (default: `https://localhost:7052`)
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)

#### UI
- `ApiBaseAddress`: Backend API base URL (auto-detected or from config)
- `ASPNETCORE_ENVIRONMENT`: Environment

### Configuration Files
- **API**: `src/API/appsettings.json` + `appsettings.Development.json`
- **UI**: `src/UI/wwwroot/appsettings.json`

---

## Security Considerations

### Current Implementation
- ✅ HTTPS in production
- ✅ CORS policy limiting frontend origin
- ✅ No hardcoded secrets
- ✅ Input validation on API layer
- ✅ Error messages sanitized (no stack traces in production)

### Future Enhancements
- 🔄 Authentication & Authorization (v1.2+)
- 🔄 Rate limiting on WoL endpoints
- 🔄 Audit logging for device wake events
- 🔄 API token support

---

## Deployment Architecture

### Docker Compose Stack
```yaml
services:
  landalf-api:
    - Container with ASP.NET Core API
    - Exposes port 5000
    - SQLite database volume

  landalf-ui:
    - Nginx reverse proxy
    - Serves WASM assets
    - Routes API requests to landalf-api

Ports:
  8080: UI (via nginx)
  5000: API
```

### Database Volume
- Path: `LANdalf_Data/landalf.db`
- Persisted between container restarts

---

## Extension Points

### Adding New Endpoints
1. Create handler method in `/src/API/Handler`
2. Map endpoint in `Program.cs`
3. Add OpenAPI documentation with summary/description
4. Regenerate UI client with NSwag

### Customizing UI
1. Edit Razor components in `/src/UI/Pages` or `/src/UI/Components`
2. Use MudBlazor components for consistency
3. Follow existing service injection patterns

### Adding Database Models
1. Create model in `/src/API/Models`
2. Add DbSet to `AppDbContext`
3. Create migration: `dotnet ef migrations add MigrationName`
4. Update services accordingly

---

## Testing Strategy

### Unit Tests
- **API Tests**: Service layer and handler logic
- **UI Tests**: Component rendering with Bunit

### Running Tests
```bash
dotnet test
```

### Coverage
Tests focus on:
- Device CRUD operations
- WoL service logic
- API endpoint contracts
- Component user interactions

---

## Performance Considerations

### Current Optimizations
- SQLite for lightweight data storage
- Minimal API endpoints (low overhead)
- WASM compilation ahead-of-time

### Potential Improvements
- Add caching layer for device queries
- Implement database indexing on MAC address
- Optimize WASM bundle size
- Lazy-load device lists

---

## Development Workflow

```
1. Clone repository
2. dotnet restore
3. dotnet build
4. dotnet test (verify all tests pass)
5. Make changes
6. Run affected tests
7. dotnet format (code formatting)
8. Create pull request
9. CI/CD pipeline validates
10. Merge and deploy
```

---

## Useful Resources

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Blazor WASM Guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MudBlazor Components](https://mudblazor.com/)
- [Wake-on-LAN Technical Details](https://en.wikipedia.org/wiki/Wake-on-LAN)


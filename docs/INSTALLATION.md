# Installation & Setup Guides

## Quick Links
- [Docker Compose (Recommended)](#docker-compose-recommended)
- [Manual Setup](#manual-setup)
- [Troubleshooting](#troubleshooting)

---

## Docker Compose (Recommended)

### Prerequisites
- Docker & Docker Compose installed ([Download](https://www.docker.com/products/docker-desktop))
- ~300 MB disk space

### Important: Host Network Mode

⚠️ **LANdalf uses Docker's `host` network mode** to enable Wake-on-LAN functionality. Magic packets (UDP broadcasts) require access to the host network to reach devices on your local network.

**What this means:**
- The application uses your host machine's network directly
- No port mapping needed (ports are used directly on the host)
- WoL magic packets are properly broadcast on your network
- Ports 5000 (API) and 80 (UI, configurable via `NGINX_PORT`) must be available on your host

**Platform Support:**
- **Linux**: ✅ Full support - host network mode works natively
- **Windows/macOS**: ⚠️ Requires Docker Desktop 4.34+ with host networking enabled (opt-in feature). See [Docker Docs](https://docs.docker.com/engine/network/drivers/host/#docker-desktop) for setup.

**Enabling Host Networking on Docker Desktop (Windows/macOS):**
1. Sign in to your Docker account in Docker Desktop
2. Navigate to **Settings**
3. Under the **Resources** tab, select **Network**
4. Check **Enable host networking**
5. Select **Apply and restart**

**⚠️ Docker Desktop Limitations** (Windows/macOS):
- Host networking works at Layer 4 (TCP/UDP) only
- Containers cannot bind to host IP addresses directly
- WoL may not work reliably due to broadcast limitations
- If WoL doesn't work, use [Manual Setup](#manual-setup) instead

### One-Command Setup

```bash
# Clone and run
git clone https://github.com/renedierking/LANdalf.git
cd LANdalf
docker compose up -d
```

### Verify Installation
```bash
# Check running containers
docker compose ps

# Should show:
# NAME                   STATUS
# api                    Up 2 minutes
# ui                     Up 2 minutes
```

### Access the Application
- **UI**: http://localhost (default port 80, configurable via `NGINX_PORT`)
- **API**: http://localhost:5000
- **API Docs**: http://localhost:5000/scalar/v1

### Port Requirements
LANdalf requires the following ports on your host:
- **5000/TCP** — API server (HTTP)
- **80/TCP** — UI server (HTTP, configurable via `NGINX_PORT` environment variable)

If port 80 is already in use, set `NGINX_PORT` to another port (e.g. `8080`) in `docker-compose.yaml` and update `Cors__FrontendUrl` to match.

### Stopping & Cleanup
```bash
# Stop containers
docker compose down

# Remove data
docker compose down -v
```

---

## Manual Setup

### Prerequisites
- [.NET 10.0 SDK or later](https://dotnet.microsoft.com/download/dotnet/10.0)
- Modern web browser (Chrome, Firefox, Edge, Safari)
- PowerShell or Bash terminal

### Step 1: Clone Repository
```bash
git clone https://github.com/renedierking/LANdalf.git
cd LANdalf
```

### Step 2: Build Project
```bash
dotnet build LANdalf.slnx
```

### Step 3: Run Tests (Optional)
```bash
dotnet test
```

### Step 4: Start API (Terminal 1)
```bash
# PowerShell
cd src/API
dotnet run

# Or from root:
dotnet run --project src/API/API.csproj
```

Expected output:
```
...
Now listening on: http://localhost:5215
Now listening on: https://localhost:7206
```

### Step 5: Start UI (Terminal 2)
```bash
# PowerShell
cd src/UI
dotnet run

# Or from root:
dotnet run --project src/UI/UI.csproj
```

Expected output:
```
...
Now listening on: http://localhost:5245
Now listening on: https://localhost:7052
```

### Step 6: Open in Browser
- **UI**: https://localhost:7052 (or http://localhost:5245)
- **API Docs**: https://localhost:7206/scalar/v1

---

## Configuration

### API Configuration

#### Environment Variables

```bash
# Linux/Mac
export Cors__FrontendUrl="https://your-frontend-url.com"

# PowerShell
$env:Cors__FrontendUrl="https://your-frontend-url.com"

# Docker (via docker-compose.yaml)
environment:
  - Cors__FrontendUrl=https://localhost:7052
```

#### Database
- **Type**: SQLite
- **Location**: `LANdalf_Data/landalf.db`
- **Auto-created**: Yes, on first run
- **Migrations**: Applied automatically on startup

### UI Configuration

#### Static Configuration (`src/UI/wwwroot/appsettings.json`)
```json
{
  "ApiBaseAddress": "https://localhost:7206"
}
```

---

## Troubleshooting

### Docker Issues

#### Problem: Port Already in Use
```
Error: bind: address already in use
```

**Solution**: Change the UI port via the `NGINX_PORT` environment variable in `docker-compose.yaml`, and update `Cors__FrontendUrl` to match:
```yaml
services:
  api:
    environment:
      - Cors__FrontendUrl=http://localhost:8080
  ui:
    environment:
      - NGINX_PORT=8080
```

#### Problem: Containers Won't Start
```bash
# Check logs
docker compose logs landalf-api
docker compose logs landalf-ui

# Full rebuild
docker compose down -v
docker compose up -d --build
```

#### Problem: Database Locked
```bash
# SQLite can lock if multiple processes access it
# Solution: Ensure only one container is running
docker compose ps
```

---

### Manual Setup Issues

#### Problem: Build Fails
```bash
# Clean and rebuild
dotnet clean LANdalf.slnx
dotnet build LANdalf.slnx
```

#### Problem: Dependencies Not Found
```bash
# Restore NuGet packages
dotnet restore LANdalf.slnx
dotnet build
```

#### Problem: API Won't Start
```
Error: ASPNETCORE_ENVIRONMENT not set
```

**Solution**: Set environment
```bash
# PowerShell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/API/API.csproj

# Bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src/API/API.csproj
```

#### Problem: CORS Errors
```
Access to XMLHttpRequest blocked by CORS policy
```

**Solution**: Ensure API and UI are on correct ports
- Default API: https://localhost:7206
- Default UI: https://localhost:7052

If URLs differ, update:
1. `src/UI/wwwroot/appsettings.json` with correct API URL
2. `src/API/appsettings.json` with correct frontend URL

---

### Network Issues

#### Problem: Cannot Add Devices
```
Error: Invalid MAC Address
```

**Solution**: MAC address must be in format: `AA:BB:CC:DD:EE:FF` or `AA-BB-CC-DD-EE-FF`

#### Problem: Devices Won't Wake
1. **Check Device BIOS**
   - Enable "Wake-on-LAN" in network adapter settings
   - May be under "Power Management" or "Advanced" tab

2. **Check Network**
   - Device must be on same subnet
   - Broadcast address must be correct
   - Router may block magic packets

3. **Debug WoL**
   ```bash
   # On Linux/Mac, test with wakeonlan tool
   brew install wakeonlan
   wakeonlan -i 192.168.1.255 AA:BB:CC:DD:EE:FF
   ```

---

## Platform-Specific Notes

### Windows
- ✅ Docker Desktop: Full support
- ✅ Manual setup: Full support
- Note: Windows Firewall may block ports—add exception or use Docker

### Linux (Ubuntu/Debian)
```bash
# Install dependencies
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0 docker.io docker-compose

# Run Docker without sudo
sudo usermod -aG docker $USER
newgrp docker
```

### macOS
```bash
# Install via Homebrew
brew install dotnet docker docker-compose

# Start Docker Desktop for Docker support
open /Applications/Docker.app
```

---

## Upgrading

### Docker
```bash
# Pull latest image
docker compose pull
docker compose down
docker compose up -d
```

### Manual Installation
```bash
git pull origin main
dotnet clean LANdalf.slnx
dotnet build LANdalf.slnx
# Restart API & UI services
```

---

## Support

- 📖 [Main Documentation](../README.md)
- 🏗️ [Architecture Guide](../ARCHITECTURE.md)
- 🐛 [Report Issues](https://github.com/renedierking/LANdalf/issues)
- 💬 [Ask Questions](https://github.com/renedierking/LANdalf/discussions)


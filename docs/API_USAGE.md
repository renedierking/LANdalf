# API Usage Guide

Complete guide for using LANdalf's REST API.

## Overview

LANdalf provides a RESTful API for managing devices and sending Wake-on-LAN commands. The API is fully documented with OpenAPI (Swagger) specification.

### Quick Links
- **Base URL**: `http://localhost:5000` (local) or `https://api.yourdomain.com` (production)
- **API Version**: v1
- **Documentation**: `http://localhost:5000/scalar/v1`
- **OpenAPI Spec**: `http://localhost:5000/openapi/v1.json`

---

## Authentication

Currently, LANdalf v1.0 has **no authentication** — it assumes trusted local network access.

**Security Note**: Don't expose the API to untrusted networks. In production:
- Use a VPN or private network
- Consider authentication implementation (planned for v1.2)
- Use reverse proxy with auth layer

---

## Base Endpoints

```
GET    /api/v1/pc-devices              → List all devices
GET    /api/v1/pc-devices/{id}         → Get one device
POST   /api/v1/pc-devices/add          → Create device
POST   /api/v1/pc-devices/{id}/set     → Update device
POST   /api/v1/pc-devices/{id}/delete  → Delete device
POST   /api/v1/pc-devices/{id}/wake    → Wake device
```

---

## Device Operations

### 1. Get All Devices

```http
GET /api/v1/pc-devices
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Gaming PC",
    "macAddress": "AA-BB-CC-DD-EE-FF",
    "ipAddress": "192.168.1.100",
    "broadcastAddress": "192.168.1.255",
    "isOnline": false
  },
  {
    "id": 2,
    "name": "Media Server",
    "macAddress": "11-22-33-44-55-66",
    "ipAddress": "192.168.1.50",
    "broadcastAddress": "192.168.1.255",
    "isOnline": true
  }
]
```

**cURL Example:**
```bash
curl http://localhost:5000/api/v1/pc-devices
```

**PowerShell Example:**
```powershell
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/pc-devices"
$response | ConvertTo-Json
```

---

### 2. Get Single Device

```http
GET /api/v1/pc-devices/{id}
```

**Parameters:**
- `id` (path, required): Device ID

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Gaming PC",
  "macAddress": "AA-BB-CC-DD-EE-FF",
  "ipAddress": "192.168.1.100",
  "broadcastAddress": "192.168.1.255",
  "isOnline": false
}
```

**Response (404 Not Found):**
```json
{
  "type": "about:blank",
  "title": "Resource not found.",
  "status": 404,
  "detail": "Device with ID 999 not found.",
  "traceId": "00-abc123..."
}
```

**cURL Example:**
```bash
curl http://localhost:5000/api/v1/pc-devices/1
```

---

### 3. Create Device

```http
POST /api/v1/pc-devices/add
Content-Type: application/json

{
  "name": "New Device",
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "ipAddress": "192.168.1.100",
  "broadcastAddress": "192.168.1.255"
}
```

**Required Fields:**
- `name`: Device name (string, max 255 chars)
- `macAddress`: MAC address (format: `AA:BB:CC:DD:EE:FF` or `AA-BB-CC-DD-EE-FF`)

**Optional Fields:**
- `ipAddress`: Device IP address (string)
- `broadcastAddress`: Network broadcast address (string)

**Response (201 Created):**
```json
{
  "id": 3,
  "name": "New Device",
  "macAddress": "AA-BB-CC-DD-EE-FF",
  "ipAddress": "192.168.1.100",
  "broadcastAddress": "192.168.1.255",
  "isOnline": false
}
```

**Response (400 Bad Request):**
```json
{
  "type": "about:blank",
  "title": "Invalid Request.",
  "status": 400,
  "detail": "The MAC address format is invalid. Use AA:BB:CC:DD:EE:FF",
  "traceId": "00-xyz789..."
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/pc-devices/add \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Device",
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "ipAddress": "192.168.1.100",
    "broadcastAddress": "192.168.1.255"
  }'
```

**PowerShell Example:**
```powershell
$body = @{
    name = "New Device"
    macAddress = "AA:BB:CC:DD:EE:FF"
    ipAddress = "192.168.1.100"
    broadcastAddress = "192.168.1.255"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/pc-devices/add" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body
```

---

### 4. Update Device

```http
POST /api/v1/pc-devices/{id}/set
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Name",
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "ipAddress": "192.168.1.101",
  "broadcastAddress": "192.168.1.255",
  "isOnline": false
}
```

**Response (204 No Content)**

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/pc-devices/1/set \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Updated Name",
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "ipAddress": "192.168.1.101",
    "broadcastAddress": "192.168.1.255",
    "isOnline": false
  }'
```

---

### 5. Delete Device

```http
POST /api/v1/pc-devices/{id}/delete
```

**Response (204 No Content)**

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/pc-devices/1/delete
```

---

### 6. Wake Device

```http
POST /api/v1/pc-devices/{id}/wake
```

Sends a magic packet to wake the specified device.

**Response (200 OK):**
```json
{
  "message": "Wake-on-LAN packet sent to Gaming PC"
}
```

**Response (404 Not Found):**
```json
{
  "type": "about:blank",
  "title": "Resource not found.",
  "status": 404,
  "detail": "Device with ID 999 not found.",
  "traceId": "00-abc123..."
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/v1/pc-devices/1/wake
```

**PowerShell Example:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/pc-devices/1/wake" `
  -Method POST
```

---

## Error Handling

All error responses follow [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807) format:

```json
{
  "type": "about:blank",
  "title": "Error Title",
  "status": 400,
  "detail": "Detailed error message",
  "traceId": "00-unique-trace-id..."
}
```

### Common Status Codes
- `200 OK`: Successful GET, POST (wake)
- `201 Created`: Successful POST (create device)
- `204 No Content`: Successful POST (set, delete)
- `400 Bad Request`: Invalid input (bad MAC format, validation error)
- `404 Not Found`: Device not found
- `500 Internal Server Error`: Server error (check logs)

---

## Usage Examples

### Complete Workflow (PowerShell)

```powershell
$baseUrl = "http://localhost:5000/api/v1"

# 1. Create device
$newDevice = @{
    name = "My Computer"
    macAddress = "AA:BB:CC:DD:EE:FF"
    ipAddress = "192.168.1.100"
    broadcastAddress = "192.168.1.255"
} | ConvertTo-Json

$device = Invoke-RestMethod -Uri "$baseUrl/pc-devices/add" `
  -Method POST `
  -ContentType "application/json" `
  -Body $newDevice

$deviceId = $device.id
Write-Host "Created device with ID: $deviceId"

# 2. Get all devices
$devices = Invoke-RestMethod -Uri "$baseUrl/pc-devices"
$devices | ForEach-Object { Write-Host $_.name }

# 3. Wake device
Invoke-RestMethod -Uri "$baseUrl/pc-devices/$deviceId/wake" -Method POST
Write-Host "Wake command sent"

# 4. Delete device
Invoke-RestMethod -Uri "$baseUrl/pc-devices/$deviceId/delete" -Method POST
Write-Host "Device deleted"
```

### Batch Operations (Bash)

```bash
API="http://localhost:5000/api/v1"

# Create multiple devices
for i in 1 2 3; do
  curl -X POST "$API/pc-devices/add" \
    -H "Content-Type: application/json" \
    -d "{
      \"name\": \"Device $i\",
      \"macAddress\": \"$(printf '%02X:%02X:%02X:%02X:%02X:%02X' $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM)\",
      \"broadcastAddress\": \"192.168.1.255\"
    }"
done

# Wake all devices
curl "$API/pc-devices" | jq '.[] | .id' | while read id; do
  curl -X POST "$API/pc-devices/$id/wake"
done
```

---

## Rate Limiting

Currently, LANdalf **does not implement rate limiting** in v1.0.

**Planned (v1.2+):**
- Rate limiting per IP
- API token quota system

---

## CORS Configuration

The API includes CORS headers for cross-origin requests:

```http
Access-Control-Allow-Origin: <Frontend URL from config>
Access-Control-Allow-Methods: GET, POST
Access-Control-Allow-Headers: Content-Type
Access-Control-Allow-Credentials: true
```

**Configuration:**
- Set `Cors:FrontendUrl` in `appsettings.json`
- Default (Development): `https://localhost:7052`

---

## API Versioning

The API uses URL-based versioning:
```
/api/v1/...    → Version 1 (current)
/api/v2/...    → Version 2 (future)
```

Current version is **v1**.

---

## API Client Libraries

### Using the Generated Client (C#/.NET)

The UI project includes an auto-generated API client (NSwag):

```csharp
using LANdalf.UI.ApiClient;

var client = new LANdalfApiClient(httpClient);

// Get all devices
var devices = await client.GetPcDevicesAsync();

// Create device
await client.CreatePcDeviceAsync(new PcDeviceDTO
{
    Name = "New Device",
    MacAddress = "AA:BB:CC:DD:EE:FF"
});

// Wake device
await client.WakePcDeviceAsync(1);
```

---

## Troubleshooting

### 404 Not Found on Valid Endpoints
- **Cause**: API running on different port
- **Solution**: Check API is running on correct port (5000)

### CORS Errors in Browser
- **Cause**: Frontend URL not configured in API
- **Solution**: Set `Cors:FrontendUrl` env var or config

### Connection Refused
- **Cause**: API not running or port blocked
- **Solution**: Verify `dotnet run` output or Docker logs

### Invalid MAC Address Error
- **Cause**: Wrong format
- **Accepted**: `AA:BB:CC:DD:EE:FF` or `AA-BB-CC-DD-EE-FF`
- **Solution**: Use colons or hyphens, uppercase preferred

---

## Support

- 📖 [Full Documentation](../README.md)
- 🏗️ [Architecture](../ARCHITECTURE.md)
- 🐛 [Report Issues](https://github.com/renedierking/LANdalf/issues)
- 💬 [Ask Questions](https://github.com/renedierking/LANdalf/discussions)


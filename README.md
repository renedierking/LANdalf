<div align="center">

# 🧙‍♂️ LANdalf

### *"You Shall Not Sleep!"*

**A Modern Wake-on-LAN Management Platform**

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

[Features](#-features) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [Contributing](#-contributing)

</div>

---

## 📖 About

LANdalf is a sleek, modern web application that brings the power of Wake-on-LAN (WoL) to your fingertips. Built with cutting-edge .NET 10.0 and Blazor WebAssembly, it provides an intuitive interface to manage and remotely wake your network devices from anywhere on your local network.

### Why LANdalf?

- 🎯 **Simple & Intuitive**: Clean, modern UI built with MudBlazor
- 🚀 **Fast**: Blazor WebAssembly for lightning-fast client-side performance
- 🐳 **Docker Ready**: One-command deployment with Docker Compose
- 🔒 **Secure**: Built on modern .NET 10.0 with best practices
- 📱 **Responsive**: Works seamlessly on desktop and mobile devices
- 🎨 **Beautiful**: State-of-the-art Material Design interface

## ✨ Features

- **Device Management**: Add, edit, and organize your network devices
- **Wake-on-LAN**: Send magic packets to wake sleeping devices remotely
- **Status Monitoring**: Real-time device online/offline status
- **MAC Address Management**: Store and manage device MAC addresses
- **IP & Broadcast Configuration**: Flexible network configuration support
- **RESTful API**: Full-featured API with OpenAPI/Swagger documentation
- **Persistent Storage**: SQLite database for reliable data storage
- **Cross-Platform**: Runs on Windows, Linux, and macOS

## 🚀 Quick Start

### Using Docker Compose (Recommended)

The fastest way to get LANdalf up and running:

```bash
# Clone the repository
git clone https://github.com/renedierking/LANdalf.git
cd LANdalf

# Start the application
docker compose up -d

# Access the application
# UI: http://localhost:8080
# API: http://localhost:5000
```

That's it! 🎉 LANdalf is now running on your network.

### Manual Setup

**Prerequisites:**
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A modern web browser (Chrome, Firefox, Edge, Safari)

**Steps:**

1. **Clone the repository**
   ```bash
   git clone https://github.com/renedierking/LANdalf.git
   cd LANdalf
   ```

2. **Run the API**
   ```bash
   cd src/API
   dotnet run
   ```
   The API will be available at `http://localhost:5000`

3. **Run the UI** (in a new terminal)
   ```bash
   cd src/UI
   dotnet run
   ```
   The UI will be available at `http://localhost:7052`

## 📚 Documentation

### Adding a Device

1. Navigate to the LANdalf web interface
2. Click the **"Add Device"** button
3. Enter the device information:
   - **Name**: A friendly name for your device
   - **MAC Address**: The physical address of the network adapter
   - **IP Address**: (Optional) The device's IP address
   - **Broadcast Address**: The network broadcast address
4. Click **"Save"**

### Waking a Device

Simply click the **"Wake"** button next to any device in your list. LANdalf will send a magic packet to wake the device.

> **Note**: Wake-on-LAN must be enabled in your device's BIOS/UEFI and network adapter settings.

### API Documentation

The API includes comprehensive OpenAPI (Swagger) documentation. When running the API, navigate to:

```
http://localhost:5000/scalar/v1
```

## 🏗️ Architecture

LANdalf follows a modern, decoupled architecture:

```
┌─────────────────────────────────────────┐
│         Blazor WebAssembly UI          │
│        (MudBlazor Components)          │
└──────────────┬──────────────────────────┘
               │ HTTP/REST
               │
┌──────────────▼──────────────────────────┐
│         ASP.NET Core API               │
│      (Versioned REST Endpoints)        │
└──────────────┬──────────────────────────┘
               │
    ┌──────────┼──────────┐
    │                     │
┌───▼────┐          ┌────▼─────┐
│ SQLite │          │   WoL    │
│   DB   │          │ Service  │
└────────┘          └──────────┘
```

### Technology Stack

**Frontend:**
- Blazor WebAssembly (.NET 10.0)
- MudBlazor UI Component Library
- Progressive Web App (PWA) Support

**Backend:**
- ASP.NET Core (.NET 10.0)
- Entity Framework Core
- SQLite Database
- API Versioning
- Scalar API Documentation

**DevOps:**
- Docker & Docker Compose
- nginx (for UI hosting in production)
- Multi-stage Docker builds

## 🛠️ Development

### Prerequisites

- .NET 10.0 SDK
- Docker & Docker Compose (for containerized development)
- Visual Studio 2022+ or VS Code with C# extension
- Git

### Building from Source

```bash
# Build the entire solution
dotnet build LANdalf.slnx

# Run tests
dotnet test

# Build Docker images
docker compose build
```

### Project Structure

```
LANdalf/
├── src/
│   ├── API/              # ASP.NET Core Web API
│   │   ├── Controllers/  # API endpoints
│   │   ├── Models/       # Data models
│   │   ├── Services/     # Business logic
│   │   └── Data/         # Database context
│   └── UI/               # Blazor WebAssembly app
│       ├── Pages/        # Razor pages
│       ├── Components/   # Reusable components
│       └── Services/     # Client services
├── test/                 # Unit and integration tests
└── docker-compose.yaml   # Docker orchestration
```

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add some amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

Please ensure your PR:
- Follows the existing code style
- Includes tests for new functionality
- Updates documentation as needed

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [.NET](https://dotnet.microsoft.com/) and [Blazor](https://blazor.net/)
- UI powered by [MudBlazor](https://mudblazor.com/)
- Inspired by the need for a modern, web-based Wake-on-LAN solution

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/renedierking/LANdalf/issues)
- **Discussions**: [GitHub Discussions](https://github.com/renedierking/LANdalf/discussions)

---

<div align="center">

Made with ❤️ by [renedierking](https://github.com/renedierking)

**If you find LANdalf helpful, please consider giving it a ⭐!**

</div>

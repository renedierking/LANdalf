# LANdalf Roadmap

## Vision

LANdalf aims to be the most intuitive and feature-rich Wake-on-LAN management platform for modern networks. We're committed to simplicity, reliability, and open-source excellence.

## Current Status: v1.0.0 (Released)

The initial public release includes core WoL functionality with a modern web interface and comprehensive API.

---

## Planned Features

### v1.1.0 (In Progress)

- [x] **Enhanced Status Monitoring**: Implement ping-based device status checks
- [ ] **Device Groups**: Organize devices into categories (e.g., "Gaming", "Work", "Media Server")
- [ ] **Scheduled Wake**: Schedule devices to wake at specific times or intervals
- [ ] **Device History**: Track wake/sleep events with timestamps

### v1.2.0 Polish & Accessibility

- [ ] **Device Notes/Description**: Free-text notes field per device (firmware version, location, owner)
- [ ] **Theme Customization**: Additional UI themes beyond current dark/light modes
- [ ] **Bulk Operations**: Wake multiple devices with a single action
- [ ] **Import/Export**: Backup and restore device configurations
- [ ] **Network Auto-Discovery**: ARP scan or mDNS discovery to suggest devices for import, reducing manual MAC entry errors

### v1.3.0 Security & Integrations

- [ ] **User Management**: Multi-user support with role-based access control (Admin/User)
- [ ] **API Token Authentication**: Secure API access for third-party integrations
- [ ] **Notification Alerts**: Webhook or email when a device goes offline unexpectedly or fails to wake after N attempts (ties into Device History)
- [ ] **Grafana / Metrics Endpoint**: Expose Prometheus-compatible metrics (online device count, ping latency averages, wake success rate) for home-lab monitoring stacks

### v1.4.0 Advanced Features

- [ ] **Health Dashboard**: Summary page showing online/offline device counts, recent wake activity, and per-group uptime percentages (builds on Device History + Groups)
- [ ] **Remote Shutdown/Sleep**: Complement WoL with SSH or WMI-based shutdown commands (opt-in per device), making it a full power lifecycle manager
- [ ] **Expanded Real-Time Events**: broadcast schedule triggers, wake attempts, and group changes via SignalR

### v2.0.0 (Future Vision)

- [ ] **Mobile Native Apps**: Dedicated iOS and Android applications
- [ ] **VPN Support**: Remote WoL capabilities beyond local network
- [ ] **Automation Rules**: Conditional device wake triggers (e.g., "wake on demand if online")
- [ ] **Integration Ecosystem**: Webhooks, IFTTT support, Home Assistant integration
- [ ] **Advanced Monitoring**: Power consumption tracking and device analytics
- [ ] **Cloud Sync**: Optional cloud backup of device configurations

---

## Enhancement Areas

### Performance

- Implement caching for frequently accessed data
- Database query optimization
- UI rendering performance improvements

### Security

- Rate limiting on API endpoints
- HTTPS enforcement in production
- Input validation hardening
- Security audit and penetration testing

### Documentation

- Comprehensive API documentation with examples
- Video tutorials for common tasks
- Troubleshooting guides by platform
- Architecture deep-dive documentation

### Community

- Plugin/extension system for developers
- Community contributions process refinement
- Regular release cycle and communication

---

## Known Limitations

- **Local Network Only**: Currently requires devices on the same network (no remote WoL via VPN)
- **Single Network Segment**: Designed for single broadcast domain
- **No Persistence Beyond Single Node**: No built-in clustering or HA setup
- **Basic BIOS Requirements**: Depends on device BIOS support for Wake-on-LAN

---

## How to Contribute

We welcome community feedback and contributions! Check the following:

- **Report Bugs**: [GitHub Issues](https://github.com/renedierking/LANdalf/issues)
- **Request Features**: [GitHub Discussions](https://github.com/renedierking/LANdalf/discussions)
- **Contribute Code**: See [CONTRIBUTING.md](CONTRIBUTING.md)

---

## Release Schedule

We aim for quarterly releases with the following pattern:

- **Major Version (v1.0 → v2.0)**: Breaking changes, significant feature sets (12-18 months)
- **Minor Version (v1.0 → v1.1)**: New features, enhancements (3-4 months)
- **Patch Version (v1.0 → v1.0.1)**: Bug fixes, security patches (as needed)

---

## Questions?

- 💬 **Discussions**: Use [GitHub Discussions](https://github.com/renedierking/LANdalf/discussions) for ideas and questions
- 🐛 **Bug Reports**: Open an [Issue](https://github.com/renedierking/LANdalf/issues) if you find a problem
- 🔒 **Security**: See [SECURITY.md](SECURITY.md) for vulnerability reporting

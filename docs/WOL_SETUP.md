# Wake-on-LAN Setup Guide

This guide helps you configure your devices and network for Wake-on-LAN (WoL) functionality with LANdalf.

## Understanding Wake-on-LAN

### How It Works

Wake-on-LAN allows you to remotely power on computers by sending a special "magic packet" over the network. The packet contains the device's MAC address and is sent via UDP broadcast.

```
Device (off)
    ↑
    │ Magic Packet
    │ (MAC: AA:BB:CC:DD:EE:FF)
    │
    └─── Network Interface (powered, listening)
         └─→ Powers on computer
```

### LANdalf & Network Mode

⚠️ **Important**: LANdalf requires **host network mode** to send WoL magic packets. This is because:
- Magic packets are sent via UDP broadcast
- Broadcast packets only work on the host network interface
- Docker's default bridge network does not support proper broadcast
- Using `network_mode: host` allows LANdalf to directly access your network

**See [Installation Guide](INSTALLATION.md#important-host-network-mode) for details.**

### Requirements
- Network interface with WoL support (most modern NICs have it)
- Firmware/BIOS with WoL enabled
- Device must be sleeping, not fully powered off
- Device must be on same network or have WoL capable router
- Power supply supporting wake signals (not in S5 state)
- Ports 5000 and 8080 available (if using Docker)

---

## Device Configuration

### Windows 10/11

#### Enable WoL in BIOS/UEFI
1. Restart and enter BIOS (usually F2, F10, Del, or Esc during boot)
2. Find **Integrated Peripherals** or **Power Management**
3. Look for:
   - "Wake on LAN"
   - "Wake on Modem"
   - "Resume by Network"
   - "Power from Network"
4. Set to **Enabled**
5. Save and exit

#### Enable in Network Adapter
1. **Open Device Manager**
   ```powershell
   devmgmt.msc
   ```

2. **Find Your Network Adapter**
   - Expand "Network adapters"
   - Right-click your adapter → **Properties**

3. **Go to Advanced Tab**
   - Look for "Wake on Magic Packet"
   - Set to **Enabled**

4. **Optional: Power Management Tab**
   - Check "Allow this device to wake the computer"
   - Check "Only Allow Magic Packet to Wake the Computer"

#### Verify Settings
```powershell
# Check WoL capability
Get-NetAdapter | Get-NetAdapterAdvancedProperty -Name "Wake*"

# Result should show WoL enabled for your adapter
```

#### Network Adapter Power Settings
1. Open **Settings** → **System** → **Power & sleep**
2. Scroll to "Related settings"
3. Click **Additional power settings**
4. Click **Change plan settings**
5. Click **Change advanced power settings**
6. Expand **Network adapter**
7. Expand **Wake on Magic Packet**
8. Set to **Enabled**

### Linux

#### Using ethtool
```bash
# Check current WoL settings
ethtool eth0

# Look for: "Wake-on: <d> (Disabled)"

# Enable WoL
sudo ethtool -s eth0 wol g

# Make persistent (Ubuntu/Debian)
sudo nano /etc/netplan/01-netcfg.yaml
```

Add to netplan configuration:
```yaml
network:
  version: 2
  ethernets:
    eth0:
      dhcp4: true
      wakeonlan: true
```

Then apply:
```bash
sudo netplan apply
```

### macOS

```bash
# Check WoL status
networksetup -getWakeOnEthernetStatus Ethernet

# Enable WoL
sudo networksetup -setWakeOnEthernetEnabled Ethernet on

# Verify
networksetup -getWakeOnEthernetStatus Ethernet
```

### Network Printer/NAS

**Check Device Documentation** - most support WoL

Common locations:
- Admin Panel → Power Management
- System Settings → Network → Wake-on-LAN
- Configuration → Advanced → WoL

---

## Network Configuration

### Finding Device MAC Address

#### Windows
```powershell
# Method 1: PowerShell
ipconfig /all

# Look for "Physical Address" under your network adapter

# Method 2: Device Settings
Settings → Network & Internet → Wi-Fi/Ethernet → Properties
# Look for "Physical address"
```

#### Linux
```bash
# Using ip command
ip link show

# Using ifconfig (legacy)
ifconfig

# Look for "HWaddr" or "ether" value
```

#### macOS
```bash
# Using networksetup
networksetup -getMAC Ethernet

# Using ifconfig
ifconfig | grep -i hwaddr
```

#### Router/DHCP
1. Log into your router (usually 192.168.1.1)
2. Find **Connected Devices** or **DHCP Clients**
3. Look up your device and note the MAC address

### Finding Broadcast Address

The broadcast address is needed to send magic packets.

#### Windows
```powershell
# Get network info
ipconfig

# Example: IP: 192.168.1.100, Subnet: 255.255.255.0
# Broadcast: 192.168.1.255 (last octet = 255)

# Or using PowerShell
$ip = [System.Net.IPAddress]"192.168.1.100"
$mask = [System.Net.IPAddress]"255.255.255.0"
# Calculate: (~($mask.Address) -bor $ip.Address) -as [System.Net.IPAddress]
```

#### Linux/macOS
```bash
# View network config
ip addr show
# or
ifconfig

# Calculate broadcast: change last octet to 255
# Example: 192.168.1.100 → 192.168.1.255
```

#### Automatic Detection
LANdalf can auto-detect broadcast addresses based on your network configuration. If not working:
1. Try explicit broadcast address
2. Verify same subnet as device
3. Check router settings

### Network Adapter Selection

For multi-homed systems (multiple network cards):
- **Ethernet**: Typically more reliable for WoL
- **Wi-Fi**: May not support WoL or require special router support
- Use dedicated port/adapter if available

---

## Testing Wake-on-LAN

### Windows Test

```powershell
# Using Test-NetConnection to send WoL (if supported)
# Or use third-party tools

# Download Wake on LAN tool:
# https://www.depicus.com/wake-on-lan/wake-on-lan-gui

# Command line (with tool installed):
wolcmd 11:22:33:44:55:66 192.168.1.255 9
```

### Linux/Mac Test

```bash
# Install wakeonlan tool
# Ubuntu/Debian
sudo apt-get install wakeonlan

# macOS
brew install wakeonlan

# Send magic packet
wakeonlan -i 192.168.1.255 AA:BB:CC:DD:EE:FF

# Test with LANdalf
# Add device in LANdalf UI with:
# MAC: AA:BB:CC:DD:EE:FF
# Broadcast: 192.168.1.255
```

### Using LANdalf

1. **Add Device**
   - Name: Test-PC
   - MAC Address: `AA:BB:CC:DD:EE:FF`
   - Broadcast: `192.168.1.255`
   - IP Address: `192.168.1.100` (optional)

2. **Put device to sleep** (not off)
   - Windows: `shutdown /h` or sleep manually
   - Linux: `systemctl suspend`

3. **Click "Wake" in LANdalf**
   - Device should power on within 5-10 seconds
   - Check console for any errors

---

## Troubleshooting WoL

### Device Won't Wake

#### Checklist
- [ ] BIOS has WoL enabled
- [ ] Network adapter has WoL enabled
- [ ] Device is sleeping (not powered off)
- [ ] Correct MAC address used
- [ ] Correct broadcast address used
- [ ] Device is on same network segment
- [ ] Magic packet actually sent (check LANdalf logs)

#### Debug Steps
```powershell
# Windows: Check WoL status
Get-NetAdapter | Where-Object Name -Like "*Ethernet*" | Get-NetAdapterAdvancedProperty -Name "Wake*"

# Look for enabled WoL property
```

### "Wake on Magic Packet Not Supported"

This means your network adapter doesn't support WoL. 

**Solutions:**
1. Check BIOS for alternative WoL settings
2. Enable different wake trigger (e.g., "Wake on Pattern Match")
3. Consider USB-powered adapter with WoL support
4. Use router-based WoL if supported

### Router Blocking Magic Packets

Some routers block broadcast packets.

**Solutions:**
1. Check router's firewall/security settings
2. Whitelist device MAC address (if available)
3. Enable "UPnP" or "NAT-PMP" if router supports it
4. Use unicast address instead of broadcast (if router supports)

### Device Sleeps Too Deep

If device won't receive magic packets:

**Windows:**
1. Settings → Power & sleep
2. Click "Additional power options"
3. Click "Change plan settings"
4. Set "Put device to sleep after" to longer interval (not 0)
5. Avoid "Hibernate" (use Sleep instead)

**Linux/Mac:**
1. Check suspend mode (S3 vs S4)
2. Ensure power supply delivers standby power
3. Check BIOS for power states

---

## Advanced Configuration

### Multiple Broadcasts

If device spans multiple subnets:
```
Main Network: 192.168.1.0/24 → Broadcast: 192.168.1.255
Guest Network: 192.168.2.0/24 → Broadcast: 192.168.2.255

# Add device twice if on different networks
```

### UDP Ports

Standard WoL ports:
- **Port 7**: Default (UDP)
- **Port 9**: Alternative (UDP)

LANdalf uses port 9 by default. If blocked:
1. Try port 7 instead
2. Check firewall rules
3. Verify port isn't blocked by antivirus

### Directed Broadcast

For directed broadcast (specific IP + port):
- More reliable across routers
- Requires target device IP
- Add device's IP address in LANdalf

---

## Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Device unreachable | Wrong broadcast | Verify broadcast = network address with last octet = 255 |
| Works sometimes | Flaky network | Use directed broadcast with device IP |
| WoL not in BIOS | Older hardware | Check BIOS version; update if available |
| Won't wake from S5 (off) | Not WoL limitation | Device must be sleeping; S5 = powered off |
| Network adapter disabled | Power saving | Disable "turn off adapter to save power" |
| Still not working | Multiple issues | Verify each prerequisite separately |

---

## Resources

- [Wake-on-LAN Wikipedia](https://en.wikipedia.org/wiki/Wake-on-LAN)
- [IEEE 802.3 Magic Packet](https://en.wikipedia.org/wiki/Wake-on-LAN#Magic_packet)
- [Wake-on-LAN Tools](https://www.depicus.com/wake-on-lan/)

---

## Support

- 🐛 [Report WoL Issues](https://github.com/renedierking/LANdalf/issues)
- 💬 [Ask for Help](https://github.com/renedierking/LANdalf/discussions)
- 📖 [Main Documentation](../README.md)


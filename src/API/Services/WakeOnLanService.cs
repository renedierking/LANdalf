using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace API.Services {
    public class WakeOnLanService {
        private readonly ILogger<WakeOnLanService> _logger;

        private const int WAKE_PORT_7 = 7;
        private const int WAKE_PORT_9 = 9;

        public WakeOnLanService(ILogger<WakeOnLanService> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task Wake(
            PhysicalAddress mac,
            IPAddress? broadcast,
            CancellationToken cancellationToken = default) {

            var macBytes = mac.GetAddressBytes();
            if (macBytes.Length != 6)
                throw new ArgumentException("Invalid MAC-Address");

            byte[] packet = new byte[6 + 16 * macBytes.Length];
            for (int i = 0; i < 6; i++) {
                packet[i] = 0xFF;
            }

            for (int i = 6; i < packet.Length; i += macBytes.Length) {
                macBytes.CopyTo(packet, i);
            }

            IEnumerable<IPAddress> targets = GetTargets(broadcast);

            using var client = new UdpClient();
            client.EnableBroadcast = true;

            foreach (IPAddress target in targets) {
                await SendAsync(packet, WAKE_PORT_7, client, target, cancellationToken);
                await SendAsync(packet, WAKE_PORT_9, client, target, cancellationToken);
            }
        }

        private async Task SendAsync(byte[] packet, int port, UdpClient client, IPAddress target, CancellationToken cancellationToken) {
            for (int i = 0; i < 3; i++) {
                cancellationToken.ThrowIfCancellationRequested();
                IPEndPoint endPoint = new IPEndPoint(target, port);
                await client.SendAsync(packet, endPoint, cancellationToken);
                _logger.LogDebug("Sent magic packet to {Target}:{Port} (attempt {Attempt}/3)", target, port, i + 1);
                await Task.Delay(30, cancellationToken);
            }
        }

        private IEnumerable<IPAddress> GetTargets(IPAddress? explicitBroadcast) {
            var results = new List<IPAddress>();

            if (explicitBroadcast != null) {
                results.Add(explicitBroadcast);
            } else {
                // If the container can see host interfaces (e.g. --network host), this method provides host broadcasts
                var detected = GetAllBroadcastAddresses().ToList();
                results.AddRange(detected);

                // Warn if the only detected addresses are in known Docker/VM NAT ranges,
                // which means magic packets will not reach the real LAN.
                if (detected.Count > 0 && detected.All(IsDockerOrVmNetwork)) {
                    _logger.LogWarning(
                        "Auto-detected broadcast addresses ({Addresses}) appear to be Docker/VM internal networks. "
                        + "WoL packets will likely NOT reach your physical LAN. "
                        + "Set the WOL_BROADCASTS environment variable to your real LAN broadcast address "
                        + "(e.g. WOL_BROADCASTS=192.168.178.255) or set a per-device broadcast address in the UI.",
                        string.Join(", ", detected));
                }
            }

            // Additional operator-configurable broadcast addresses (e.g. WOL_BROADCASTS="192.168.1.255,10.0.0.255")
            string? env = Environment.GetEnvironmentVariable("WOL_BROADCASTS");
            if (!string.IsNullOrWhiteSpace(env)) {
                foreach (string part in env.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
                    if (IPAddress.TryParse(part, out IPAddress? ip) && ip?.AddressFamily == AddressFamily.InterNetwork) {
                        if (!results.Any(r => r.Equals(ip))) {
                            results.Add(ip);
                        }
                    } else {
                        _logger.LogWarning("Ignoring invalid address in WOL_BROADCASTS: {Address}", part);
                    }
                }
            }

            // Fallback: global broadcast
            if (!results.Any()) {
                results.Add(IPAddress.Broadcast); // 255.255.255.255
            }

            _logger.LogInformation("WoL broadcast targets: {Targets}", string.Join(", ", results));

            return results;
        }

        private static bool IsDockerOrVmNetwork(IPAddress address) {
            byte[] bytes = address.GetAddressBytes();
            if (bytes.Length != 4) return false;

            // Docker Desktop NAT: 192.168.65.0/24
            if (bytes[0] == 192 && bytes[1] == 168 && bytes[2] == 65) return true;

            // Docker default bridge: 172.17.0.0/16
            if (bytes[0] == 172 && bytes[1] == 17) return true;

            // Common Docker bridge networks: 172.18-31.0.0/16
            if (bytes[0] == 172 && bytes[1] >= 18 && bytes[1] <= 31) return true;

            return false;
        }

        private IEnumerable<IPAddress> GetAllBroadcastAddresses() {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            HashSet<string> unique = new HashSet<string>();
            List<IPAddress> results = new List<IPAddress>();

            foreach (NetworkInterface ni in interfaces) {
                if (ni.OperationalStatus != OperationalStatus.Up) {
                    continue;
                }
                if (!ni.Supports(NetworkInterfaceComponent.IPv4)) {
                    continue;
                }
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) {
                    continue;
                }

                IPInterfaceProperties properties = ni.GetIPProperties();
                foreach (UnicastIPAddressInformation unicast in properties.UnicastAddresses) {
                    if (unicast.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    IPAddress? mask = unicast.IPv4Mask;
                    if (mask == null)
                        continue;

                    byte[] ipBytes = unicast.Address.GetAddressBytes();
                    byte[] maskBytes = mask.GetAddressBytes();
                    if (ipBytes.Length != maskBytes.Length)
                        continue;

                    byte[] broadcastBytes = new byte[ipBytes.Length];
                    for (int i = 0; i < ipBytes.Length; i++) {
                        broadcastBytes[i] = (byte)(ipBytes[i] | (~maskBytes[i]));
                    }

                    IPAddress broadcastAddress = new IPAddress(broadcastBytes);
                    if (unique.Add(broadcastAddress.ToString())) {
                        results.Add(broadcastAddress);
                    }
                }
            }

            return results;
        }
    }
}

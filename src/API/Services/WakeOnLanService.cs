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

            _logger.LogInformation("Preparing WoL magic packet for MAC {MacAddress}", mac);

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
            _logger.LogInformation("Resolved {TargetCount} broadcast target(s): {Targets}", targets.Count(), string.Join(", ", targets));

            using var client = new UdpClient();
            client.EnableBroadcast = true;

            foreach (IPAddress target in targets) {
                await SendAsync(packet, WAKE_PORT_7, client, target, cancellationToken);
                await SendAsync(packet, WAKE_PORT_9, client, target, cancellationToken);
            }

            _logger.LogInformation("WoL magic packet sent successfully for MAC {MacAddress}", mac);
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
                _logger.LogDebug("Using explicit broadcast address: {BroadcastAddress}", explicitBroadcast);
                results.Add(explicitBroadcast);
            } else {
                _logger.LogDebug("No explicit broadcast address, auto-detecting from network interfaces");
                // If the container can see host interfaces (e.g. --network host), this method provides host broadcasts
                results.AddRange(GetAllBroadcastAddresses());
            }

            // Additional operator-configurable broadcast addresses (e.g. WOL_BROADCASTS="192.168.1.255,10.0.0.255")
            string? env = Environment.GetEnvironmentVariable("WOL_BROADCASTS");
            if (!string.IsNullOrWhiteSpace(env)) {
                _logger.LogDebug("WOL_BROADCASTS environment variable found: {WolBroadcasts}", env);
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
                _logger.LogDebug("No broadcast addresses resolved, falling back to 255.255.255.255");
                results.Add(IPAddress.Broadcast); // 255.255.255.255
            }

            return results;
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
                        _logger.LogDebug("Detected broadcast address {BroadcastAddress} on interface {InterfaceName}", broadcastAddress, ni.Name);
                        results.Add(broadcastAddress);
                    }
                }
            }

            return results;
        }
    }
}

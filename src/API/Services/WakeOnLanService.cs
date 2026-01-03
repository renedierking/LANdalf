using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace API.Services {
    public class WakeOnLanService {
        public async Task Wake(
            PhysicalAddress mac,
            IPAddress broadcast,
            int port = 9,
            CancellationToken cancellationToken = default) {

            var macBytes = mac.GetAddressBytes();
            if (macBytes.Length != 6)
                throw new ArgumentException("Ungültige MAC-Adresse");

            byte[] packet = new byte[6 + 16 * macBytes.Length];
            for (int i = 0; i < 6; i++) {
                packet[i] = 0xFF;
            }

            for (int i = 6; i < packet.Length; i += macBytes.Length) {
                macBytes.CopyTo(packet, i);
            }

            using var client = new UdpClient();
            client.EnableBroadcast = true;

            for (int i = 0; i < 3; i++) {
                await client.SendAsync(packet, broadcast.ToString(), port, cancellationToken);
                await Task.Delay(100);
            }
        }
    }
}

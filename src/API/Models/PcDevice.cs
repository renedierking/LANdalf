using System.Net;
using System.Net.NetworkInformation;

namespace API.Models {
    public class PcDevice {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public PhysicalAddress MacAddress { get; set; } = PhysicalAddress.None;
        public IPAddress? IpAddress { get; set; } = null;
        public IPAddress? BroadcastAddress { get; set; } = null;
        public bool IsOnline { get; set; }
    }
}

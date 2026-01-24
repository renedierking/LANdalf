using API.Models;
using LANdalf.API.DTOs;

namespace LANdalf.API.Extensions {
    public static class DtoExtensions {
        public static PcDeviceDTO ToDto(this PcDevice pcDevice) =>
            new PcDeviceDTO(
                Id: pcDevice.Id,
                Name: pcDevice.Name,
                MacAddress: string.Join("-", pcDevice.MacAddress.GetAddressBytes().Select(b => b.ToString("X2"))),
                IpAddress: pcDevice.IpAddress?.ToString(),
                BroadcastAddress: pcDevice.BroadcastAddress?.ToString(),
                IsOnline: pcDevice.IsOnline
            );
    }
}

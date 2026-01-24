namespace LANdalf.API.DTOs {
    public record PcDeviceDTO(
        int Id,
        string Name,
        string MacAddress,
        string? IpAddress,
        string? BroadcastAddress,
        bool IsOnline
    );
}

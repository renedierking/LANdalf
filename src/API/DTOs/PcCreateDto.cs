namespace API.DTOs {
    public record PcCreateDto(
        string Name,
        string MacAddress,
        string IpAddress,
        string BroadcastAddress
    );
}

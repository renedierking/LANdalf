namespace LANdalf.API.DTOs {
    public record DeviceEventDTO(
        int Id,
        int PcDeviceId,
        string PcDeviceName,
        string EventType,
        DateTime Timestamp,
        string? Details
    );
}

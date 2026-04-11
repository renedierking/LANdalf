namespace LANdalf.UI.ApiClient {
    public partial class DeviceEventDTO {
        public int Id { get; set; }
        public int PcDeviceId { get; set; }
        public string PcDeviceName { get; set; } = "";
        public string EventType { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }
}

namespace API.Models {
    public class DeviceEvent {
        public int Id { get; set; }
        public int PcDeviceId { get; set; }
        public string EventType { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }

        // Navigation property
        public PcDevice? PcDevice { get; set; }
    }
}

namespace API.Services {
    public class DeviceMonitoringOptions {
        public const string SectionName = "DeviceMonitoring";

        /// <summary>
        /// Gets or sets whether device monitoring is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the interval in seconds between device status checks.
        /// Minimum value is 5 seconds.
        /// </summary>
        public int IntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the ping timeout in milliseconds.
        /// Minimum value is 100 milliseconds.
        /// </summary>
        public int TimeoutMilliseconds { get; set; } = 2000;
    }
}

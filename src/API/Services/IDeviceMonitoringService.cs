namespace API.Services {
    public interface IDeviceMonitoringService {
        /// <summary>
        /// Gets whether the monitoring service is currently enabled and running.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets the configured monitoring interval in seconds.
        /// </summary>
        int IntervalSeconds { get; }

        /// <summary>
        /// Gets the configured ping timeout in milliseconds.
        /// </summary>
        int TimeoutMilliseconds { get; }

        /// <summary>
        /// Manually triggers a device status check for all devices.
        /// </summary>
        Task CheckAllDevicesAsync(CancellationToken cancellationToken = default);
    }
}

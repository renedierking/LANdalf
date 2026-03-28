using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;

namespace API.Services {
    public class DeviceMonitoringService : BackgroundService, IDeviceMonitoringService {
        private readonly ILogger<DeviceMonitoringService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly DeviceMonitoringOptions _options;

        public bool IsEnabled => _options.Enabled;
        public int IntervalSeconds => _options.IntervalSeconds;
        public int TimeoutMilliseconds => _options.TimeoutMilliseconds;

        public DeviceMonitoringService(
            ILogger<DeviceMonitoringService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<DeviceMonitoringOptions> options) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _options = options.Value;

            // Validate configuration
            if (_options.IntervalSeconds < 5) {
                _logger.LogWarning("DeviceMonitoring:IntervalSeconds is too low ({Interval}s), using minimum of 5s", _options.IntervalSeconds);
                _options.IntervalSeconds = 5;
            }
            if (_options.TimeoutMilliseconds < 100) {
                _logger.LogWarning("DeviceMonitoring:TimeoutMilliseconds is too low ({Timeout}ms), using minimum of 100ms", _options.TimeoutMilliseconds);
                _options.TimeoutMilliseconds = 100;
            }

            _logger.LogInformation(
                "DeviceMonitoringService configured: Enabled={Enabled}, Interval={Interval}s, Timeout={Timeout}ms",
                _options.Enabled, _options.IntervalSeconds, _options.TimeoutMilliseconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            if (!IsEnabled) {
                _logger.LogInformation("Device monitoring is disabled");
                return;
            }

            _logger.LogInformation("Device monitoring service started");

            // Wait a bit before starting the first check to allow the application to fully initialize
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested) {
                try {
                    await CheckAllDevicesAsync(stoppingToken);
                } catch (OperationCanceledException) {
                    // Expected when stopping
                    break;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error during device monitoring cycle");
                }

                try {
                    await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
                } catch (OperationCanceledException) {
                    // Expected when stopping
                    break;
                }
            }

            _logger.LogInformation("Device monitoring service stopped");
        }

        public async Task CheckAllDevicesAsync(CancellationToken cancellationToken = default) {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbService = scope.ServiceProvider.GetRequiredService<IAppDbService>();

            var devices = await dbService.GetAllPcDevicesAsync(cancellationToken);
            var deviceList = devices.ToList();

            if (deviceList.Count == 0) {
                _logger.LogDebug("No devices to monitor");
                return;
            }

            _logger.LogDebug("Checking online status for {Count} device(s)", deviceList.Count);

            // Check all devices in parallel for better performance
            var tasks = deviceList.Select(device => CheckDeviceAsync(device, dbService, cancellationToken));
            await Task.WhenAll(tasks);

            _logger.LogDebug("Completed status check for {Count} device(s)", deviceList.Count);
        }

        private async Task CheckDeviceAsync(
            Models.PcDevice device,
            IAppDbService dbService,
            CancellationToken cancellationToken) {

            // Skip devices without an IP address
            if (device.IpAddress == null) {
                _logger.LogDebug("Device {Name} (ID={Id}) has no IP address, skipping ping", device.Name, device.Id);

                // If device was previously online, mark it as offline
                if (device.IsOnline) {
                    device.IsOnline = false;
                    await dbService.UpdatePcDeviceAsync(device, cancellationToken);
                    _logger.LogInformation("Device {Name} (ID={Id}) marked offline (no IP address)", device.Name, device.Id);
                }
                return;
            }

            bool isOnline = await PingDeviceAsync(device.IpAddress, cancellationToken);

            // Only update if status changed
            if (device.IsOnline != isOnline) {
                device.IsOnline = isOnline;
                await dbService.UpdatePcDeviceAsync(device, cancellationToken);

                string status = isOnline ? "ONLINE" : "OFFLINE";
                _logger.LogInformation(
                    "Device {Name} (ID={Id}, IP={IpAddress}) is now {Status}",
                    device.Name, device.Id, device.IpAddress, status);
            } else {
                _logger.LogDebug(
                    "Device {Name} (ID={Id}, IP={IpAddress}) status unchanged: {Status}",
                    device.Name, device.Id, device.IpAddress, isOnline ? "online" : "offline");
            }
        }

        private async Task<bool> PingDeviceAsync(System.Net.IPAddress ipAddress, CancellationToken cancellationToken) {
            try {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, _options.TimeoutMilliseconds);
                return reply.Status == IPStatus.Success;
            } catch (PingException ex) {
                _logger.LogDebug(ex, "Ping failed for {IpAddress}", ipAddress);
                return false;
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Unexpected error pinging {IpAddress}", ipAddress);
                return false;
            }
        }
    }
}

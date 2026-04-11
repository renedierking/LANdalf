using Microsoft.Extensions.Options;

namespace API.Services {
    public interface IWakeScheduleService {
        bool IsEnabled { get; }
        int CheckIntervalSeconds { get; }
    }

    public class WakeScheduleService : BackgroundService, IWakeScheduleService {
        private readonly ILogger<WakeScheduleService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly WakeScheduleOptions _options;

        public bool IsEnabled => _options.Enabled;
        public int CheckIntervalSeconds => _options.CheckIntervalSeconds;

        public WakeScheduleService(
            ILogger<WakeScheduleService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<WakeScheduleOptions> options) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _options = options.Value;

            // Validate configuration
            if (_options.CheckIntervalSeconds < 10) {
                _logger.LogWarning("WakeSchedule:CheckIntervalSeconds is too low ({Interval}s), using minimum of 10s", _options.CheckIntervalSeconds);
                _options.CheckIntervalSeconds = 10;
            }

            _logger.LogInformation(
                "WakeScheduleService configured: Enabled={Enabled}, CheckInterval={Interval}s",
                _options.Enabled, _options.CheckIntervalSeconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            if (!IsEnabled) {
                _logger.LogInformation("Wake schedule service is disabled");
                return;
            }

            _logger.LogInformation("Wake schedule service started");

            // Wait a bit before starting to allow the application to fully initialize
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested) {
                try {
                    await ProcessSchedulesAsync(stoppingToken);
                } catch (OperationCanceledException) {
                    // Expected when stopping
                    break;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error during wake schedule processing cycle");
                }

                try {
                    await Task.Delay(TimeSpan.FromSeconds(_options.CheckIntervalSeconds), stoppingToken);
                } catch (OperationCanceledException) {
                    // Expected when stopping
                    break;
                }
            }

            _logger.LogInformation("Wake schedule service stopped");
        }

        private async Task ProcessSchedulesAsync(CancellationToken cancellationToken) {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbService = scope.ServiceProvider.GetRequiredService<IAppDbService>();
            var wolService = scope.ServiceProvider.GetRequiredService<WakeOnLanService>();

            var schedules = await dbService.GetAllWakeSchedulesAsync(cancellationToken);
            var scheduleList = schedules.Where(s => s.Enabled).ToList();

            if (scheduleList.Count == 0) {
                _logger.LogDebug("No enabled schedules to process");
                return;
            }

            _logger.LogDebug("Processing {Count} enabled schedule(s)", scheduleList.Count);

            var utcNow = DateTime.UtcNow;

            foreach (var schedule in scheduleList) {
                try {
                    // Update next execution if it's not set
                    if (schedule.NextExecution == null) {
                        WakeScheduleHelper.UpdateScheduleExecution(schedule, utcNow);
                        await dbService.UpdateWakeScheduleAsync(schedule, cancellationToken);
                    }

                    // Check if schedule should execute
                    if (WakeScheduleHelper.ShouldExecute(schedule, utcNow)) {
                        await ExecuteScheduleAsync(schedule, dbService, wolService, cancellationToken);
                    }
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error processing schedule {ScheduleId} for device {DeviceId}",
                        schedule.Id, schedule.PcDeviceId);
                }
            }
        }

        private async Task ExecuteScheduleAsync(
            Models.WakeSchedule schedule,
            IAppDbService dbService,
            WakeOnLanService wolService,
            CancellationToken cancellationToken) {

            var device = schedule.PcDevice;
            if (device == null) {
                device = await dbService.GetPcDeviceByIdAsync(schedule.PcDeviceId, cancellationToken);
                if (device == null) {
                    _logger.LogWarning("Device {DeviceId} not found for schedule {ScheduleId}",
                        schedule.PcDeviceId, schedule.Id);
                    return;
                }
            }

            _logger.LogInformation(
                "Executing scheduled wake for device {DeviceName} (ID={DeviceId}, Schedule={ScheduleId})",
                device.Name, device.Id, schedule.Id);

            try {
                // Send WOL packet
                await wolService.Wake(device.MacAddress, device.BroadcastAddress, cancellationToken);

                _logger.LogInformation(
                    "Scheduled wake executed successfully for device {DeviceName} (ID={DeviceId})",
                    device.Name, device.Id);

                // Mark schedule as executed
                var utcNow = DateTime.UtcNow;
                WakeScheduleHelper.MarkAsExecuted(schedule, utcNow);
                await dbService.UpdateWakeScheduleAsync(schedule, cancellationToken);

            } catch (Exception ex) {
                _logger.LogError(ex,
                    "Failed to execute scheduled wake for device {DeviceName} (ID={DeviceId})",
                    device.Name, device.Id);
            }
        }
    }
}

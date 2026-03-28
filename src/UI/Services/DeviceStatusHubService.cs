using Microsoft.AspNetCore.SignalR.Client;
using LANdalf.UI.ApiClient;

namespace LANdalf.UI.Services {
    public class DeviceStatusHubService : IAsyncDisposable {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<DeviceStatusHubService> _logger;

        public event Func<PcDeviceDTO, Task>? OnDeviceStatusChanged;

        public DeviceStatusHubService(IConfiguration configuration, ILogger<DeviceStatusHubService> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var apiUrl = configuration["ApiUrl"] ?? "https://localhost:7179";
            var hubUrl = $"{apiUrl}/hubs/devicestatus";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<PcDeviceDTO>("DeviceStatusChanged", async (device) => {
                _logger.LogDebug("Received device status change for {DeviceName} (ID={DeviceId})", device.Name, device.Id);
                if (OnDeviceStatusChanged != null) {
                    await OnDeviceStatusChanged.Invoke(device);
                }
            });

            _hubConnection.Reconnecting += error => {
                _logger.LogWarning(error, "SignalR connection lost. Reconnecting...");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId => {
                _logger.LogInformation("SignalR reconnected with connection ID: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += error => {
                _logger.LogError(error, "SignalR connection closed");
                return Task.CompletedTask;
            };
        }

        public async Task StartAsync() {
            try {
                if (_hubConnection.State == HubConnectionState.Disconnected) {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("SignalR connection started successfully");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to start SignalR connection");
            }
        }

        public async Task StopAsync() {
            try {
                if (_hubConnection.State != HubConnectionState.Disconnected) {
                    await _hubConnection.StopAsync();
                    _logger.LogInformation("SignalR connection stopped");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to stop SignalR connection");
            }
        }

        public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

        public async ValueTask DisposeAsync() {
            await _hubConnection.DisposeAsync();
        }
    }
}

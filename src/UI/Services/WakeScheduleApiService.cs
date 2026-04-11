using System.Net.Http.Json;
using System.Text.Json;

namespace LANdalf.UI.Services {
    public interface IWakeScheduleApiService {
        Task<Result<IReadOnlyList<WakeScheduleDTO>, Exception>> GetAllWakeSchedulesAsync(CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<WakeScheduleDTO>, Exception>> GetWakeSchedulesByDeviceAsync(int deviceId, CancellationToken cancellationToken = default);
        Task<Result<WakeScheduleDTO, Exception>> GetWakeScheduleByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<WakeScheduleDTO, Exception>> AddWakeScheduleAsync(WakeScheduleCreateDto dto, CancellationToken cancellationToken = default);
        Task<Result<bool, Exception>> UpdateWakeScheduleAsync(int id, WakeScheduleCreateDto dto, CancellationToken cancellationToken = default);
        Task<Result<bool, Exception>> DeleteWakeScheduleAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool, Exception>> ToggleWakeScheduleAsync(int id, CancellationToken cancellationToken = default);
    }

    public class WakeScheduleApiService : IWakeScheduleApiService {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public WakeScheduleApiService(HttpClient httpClient) {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<Result<IReadOnlyList<WakeScheduleDTO>, Exception>> GetAllWakeSchedulesAsync(CancellationToken cancellationToken = default) {
            try {
                var schedules = await _httpClient.GetFromJsonAsync<List<WakeScheduleDTO>>(
                    "api/v1/wake-schedules/", _jsonOptions, cancellationToken);
                return schedules ?? new List<WakeScheduleDTO>();
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<IReadOnlyList<WakeScheduleDTO>, Exception>> GetWakeSchedulesByDeviceAsync(int deviceId, CancellationToken cancellationToken = default) {
            try {
                var schedules = await _httpClient.GetFromJsonAsync<List<WakeScheduleDTO>>(
                    $"api/v1/pc-devices/{deviceId}/wake-schedules/", _jsonOptions, cancellationToken);
                return schedules ?? new List<WakeScheduleDTO>();
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<WakeScheduleDTO, Exception>> GetWakeScheduleByIdAsync(int id, CancellationToken cancellationToken = default) {
            try {
                var schedule = await _httpClient.GetFromJsonAsync<WakeScheduleDTO>(
                    $"api/v1/wake-schedules/{id}", _jsonOptions, cancellationToken);
                return schedule ?? throw new Exception("Schedule not found");
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<WakeScheduleDTO, Exception>> AddWakeScheduleAsync(WakeScheduleCreateDto dto, CancellationToken cancellationToken = default) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/v1/wake-schedules/add", dto, _jsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();
                var schedule = await response.Content.ReadFromJsonAsync<WakeScheduleDTO>(_jsonOptions, cancellationToken);
                return schedule ?? throw new Exception("Failed to create schedule");
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<bool, Exception>> UpdateWakeScheduleAsync(int id, WakeScheduleCreateDto dto, CancellationToken cancellationToken = default) {
            try {
                var response = await _httpClient.PostAsJsonAsync($"api/v1/wake-schedules/{id}/set", dto, _jsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();
                return true;
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<bool, Exception>> DeleteWakeScheduleAsync(int id, CancellationToken cancellationToken = default) {
            try {
                var response = await _httpClient.PostAsync($"api/v1/wake-schedules/{id}/delete", null, cancellationToken);
                response.EnsureSuccessStatusCode();
                return true;
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<bool, Exception>> ToggleWakeScheduleAsync(int id, CancellationToken cancellationToken = default) {
            try {
                var response = await _httpClient.PostAsync($"api/v1/wake-schedules/{id}/toggle", null, cancellationToken);
                response.EnsureSuccessStatusCode();
                return true;
            } catch (Exception ex) {
                return ex;
            }
        }
    }

    // DTOs for UI
    public record WakeScheduleDTO(
        int Id,
        int PcDeviceId,
        string PcDeviceName,
        string ScheduledTime,
        string? DaysOfWeek,
        bool Enabled,
        DateTimeOffset? LastExecuted,
        DateTimeOffset? NextExecution,
        string CronExpression
    );

    public class WakeScheduleCreateDto {
        public int PcDeviceId { get; set; }
        public string ScheduledTime { get; set; } = "";
        public string? DaysOfWeek { get; set; }
        public bool Enabled { get; set; }
    }
}

using LANdalf.UI.ApiClient;
using System.Net.Http.Json;

namespace LANdalf.UI.Services {
    public class LANdalfApiService : ILANdalfApiService {
        private readonly LANdalfApiClient _apiClient;
        private readonly HttpClient _httpClient;

        public LANdalfApiService(LANdalfApiClient apiClient, HttpClient httpClient) {
            _apiClient = apiClient;
            _httpClient = httpClient;
        }

        public async Task<Result<IReadOnlyList<PcDeviceDTO>, Exception>> GetAllPcDevicesAsync(CancellationToken cancellationToken = default) {
            try {
                var pcDevices = await _apiClient.GetAllPcDevicesAsync(cancellationToken);
                return pcDevices.ToList();
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<PcDeviceDTO, Exception>> GetPcDeviceByIdAsync(int id, CancellationToken cancellationToken = default) {
            try {
                var pcDevice = await _apiClient.GetPcDeviceByIdAsync(id, cancellationToken);
                return pcDevice;
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<bool, Exception>> AddPcDeviceAsync(PcCreateDto dto, CancellationToken cancellationToken = default) {
            try {
                await _apiClient.AddPcDeviceAsync(dto, cancellationToken);
                return true;
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<bool, Exception>> UpdatePcDevice(PcDeviceDTO dto, CancellationToken cancellationToken = default) {
            try {
                await _apiClient.SetPcDeviceAsync(dto.Id, dto, cancellationToken);
                return true;
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<bool, Exception>> DeletePcDeviceAsync(int id, CancellationToken cancellationToken = default) {
            try {
                await _apiClient.DeletePcDeviceAsync(id, cancellationToken);
                return true;
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<bool, Exception>> WakePcDeviceAsync(int id, CancellationToken cancellationToken = default) {
            try {
                await _apiClient.WakePcDeviceAsync(id, cancellationToken);
                return true;
            } catch (Exception ex) {
                return ex;
            }
        }

        public async Task<Result<IReadOnlyList<DeviceEventDTO>, Exception>> GetDeviceHistoryAsync(int deviceId, int limit = 50, int offset = 0, CancellationToken cancellationToken = default) {
            try {
                var response = await _httpClient.GetAsync(
                    $"api/v1/pc-devices/{deviceId}/history?limit={limit}&offset={offset}",
                    cancellationToken);

                if (response.IsSuccessStatusCode) {
                    var events = await response.Content.ReadFromJsonAsync<List<DeviceEventDTO>>(cancellationToken);
                    return events ?? new List<DeviceEventDTO>();
                }

                throw new Exception($"Failed to fetch device history: {response.StatusCode}");
            } catch (Exception ex) {
                return ex;
            }
        }
    }
}

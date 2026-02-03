using LANdalf.UI.ApiClient;

namespace LANdalf.UI.Services {
    public class LANdalfApiService : ILANdalfApiService {
        private readonly LANdalfApiClient _apiClient;
        public LANdalfApiService(LANdalfApiClient apiClient) {
            _apiClient = apiClient;
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
    }
}

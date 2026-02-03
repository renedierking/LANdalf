using LANdalf.UI.ApiClient;

namespace LANdalf.UI.Services {
    public interface ILANdalfApiService {
        Task<Result<bool, Exception>> AddPcDeviceAsync(PcCreateDto dto, CancellationToken cancellationToken = default);
        Task<Result<bool, Exception>> DeletePcDeviceAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<PcDeviceDTO>, Exception>> GetAllPcDevicesAsync(CancellationToken cancellationToken = default);
        Task<Result<PcDeviceDTO, Exception>> GetPcDeviceByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool, Exception>> UpdatePcDevice(PcDeviceDTO dto, CancellationToken cancellationToken = default);
        Task<Result<bool, Exception>> WakePcDeviceAsync(int id, CancellationToken cancellationToken = default);
    }
}

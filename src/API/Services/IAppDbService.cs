using API.Models;

namespace API.Services {
    public interface IAppDbService {
        /// <summary>
        /// Gets all PC devices from the database
        /// </summary>
        Task<IEnumerable<PcDevice>> GetAllPcDevicesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a PC device by its ID
        /// </summary>
        Task<PcDevice?> GetPcDeviceByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new PC device in the database
        /// </summary>
        Task<PcDevice> CreatePcDeviceAsync(PcDevice device, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing PC device
        /// </summary>
        Task<PcDevice> UpdatePcDeviceAsync(PcDevice device, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a PC device by its ID
        /// </summary>
        Task<bool> DeletePcDeviceAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all wake schedules from the database
        /// </summary>
        Task<IEnumerable<WakeSchedule>> GetAllWakeSchedulesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets wake schedules for a specific device
        /// </summary>
        Task<IEnumerable<WakeSchedule>> GetWakeSchedulesByDeviceIdAsync(int deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a wake schedule by its ID
        /// </summary>
        Task<WakeSchedule?> GetWakeScheduleByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new wake schedule
        /// </summary>
        Task<WakeSchedule> CreateWakeScheduleAsync(WakeSchedule schedule, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing wake schedule
        /// </summary>
        Task<WakeSchedule> UpdateWakeScheduleAsync(WakeSchedule schedule, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a wake schedule by its ID
        /// </summary>
        Task<bool> DeleteWakeScheduleAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all device events from the database
        /// </summary>
        Task<IEnumerable<DeviceEvent>> GetAllDeviceEventsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets device events for a specific device with pagination
        /// </summary>
        Task<IEnumerable<DeviceEvent>> GetDeviceEventsByDeviceIdAsync(int deviceId, int limit = 50, int offset = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a device event by its ID
        /// </summary>
        Task<DeviceEvent?> GetDeviceEventByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new device event
        /// </summary>
        Task<DeviceEvent> CreateDeviceEventAsync(DeviceEvent deviceEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a device event by its ID
        /// </summary>
        Task<bool> DeleteDeviceEventAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

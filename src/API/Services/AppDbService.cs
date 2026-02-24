using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services {
    public class AppDbService : IAppDbService {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AppDbService> _logger;

        public AppDbService(AppDbContext dbContext, ILogger<AppDbService> logger) {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<PcDevice>> GetAllPcDevicesAsync(CancellationToken cancellationToken = default) {
            _logger.LogDebug("Fetching all PC devices from database");
            return await _dbContext.PcDevices.ToListAsync(cancellationToken);
        }

        public async Task<PcDevice?> GetPcDeviceByIdAsync(int id, CancellationToken cancellationToken = default) {
            _logger.LogDebug("Fetching PC device {DeviceId} from database", id);
            return await _dbContext.PcDevices.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<PcDevice> CreatePcDeviceAsync(PcDevice device, CancellationToken cancellationToken = default) {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            _logger.LogDebug("Inserting new device {DeviceName} into database", device.Name);
            var entry = await _dbContext.PcDevices.AddAsync(device, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Inserted device {DeviceId} ({DeviceName})", entry.Entity.Id, entry.Entity.Name);
            return entry.Entity;
        }

        public async Task<PcDevice> UpdatePcDeviceAsync(PcDevice device, CancellationToken cancellationToken = default) {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            _logger.LogDebug("Updating device {DeviceId} in database", device.Id);
            _dbContext.PcDevices.Update(device);
            await SaveChangesAsync(cancellationToken);
            return device;
        }

        public async Task<bool> DeletePcDeviceAsync(int id, CancellationToken cancellationToken = default) {
            _logger.LogDebug("Deleting device {DeviceId} from database", id);
            var device = await GetPcDeviceByIdAsync(id, cancellationToken);
            if (device == null) {
                _logger.LogWarning("Device {DeviceId} not found for deletion", id);
                return false;
            }

            _dbContext.PcDevices.Remove(device);
            await SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Deleted device {DeviceId}", id);
            return true;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

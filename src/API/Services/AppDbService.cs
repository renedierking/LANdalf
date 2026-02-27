using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services {
    public class AppDbService : IAppDbService {
        private readonly AppDbContext _dbContext;

        public AppDbService(AppDbContext dbContext) {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<PcDevice>> GetAllPcDevicesAsync(CancellationToken cancellationToken = default) {
            return await _dbContext.PcDevices.ToListAsync(cancellationToken);
        }

        public async Task<PcDevice?> GetPcDeviceByIdAsync(int id, CancellationToken cancellationToken = default) {
            return await _dbContext.PcDevices.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<PcDevice> CreatePcDeviceAsync(PcDevice device, CancellationToken cancellationToken = default) {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            var entry = await _dbContext.PcDevices.AddAsync(device, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return entry.Entity;
        }

        public async Task<PcDevice> UpdatePcDeviceAsync(PcDevice device, CancellationToken cancellationToken = default) {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            _dbContext.PcDevices.Update(device);
            await SaveChangesAsync(cancellationToken);
            return device;
        }

        public async Task<bool> DeletePcDeviceAsync(int id, CancellationToken cancellationToken = default) {
            var device = await GetPcDeviceByIdAsync(id, cancellationToken);
            if (device == null) {
                return false;
            }

            _dbContext.PcDevices.Remove(device);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

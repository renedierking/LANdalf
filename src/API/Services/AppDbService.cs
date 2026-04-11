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

        public async Task<IEnumerable<WakeSchedule>> GetAllWakeSchedulesAsync(CancellationToken cancellationToken = default) {
            return await _dbContext.WakeSchedules
                .Include(ws => ws.PcDevice)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<WakeSchedule>> GetWakeSchedulesByDeviceIdAsync(int deviceId, CancellationToken cancellationToken = default) {
            return await _dbContext.WakeSchedules
                .Include(ws => ws.PcDevice)
                .Where(ws => ws.PcDeviceId == deviceId)
                .ToListAsync(cancellationToken);
        }

        public async Task<WakeSchedule?> GetWakeScheduleByIdAsync(int id, CancellationToken cancellationToken = default) {
            return await _dbContext.WakeSchedules
                .Include(ws => ws.PcDevice)
                .FirstOrDefaultAsync(ws => ws.Id == id, cancellationToken);
        }

        public async Task<WakeSchedule> CreateWakeScheduleAsync(WakeSchedule schedule, CancellationToken cancellationToken = default) {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            var entry = await _dbContext.WakeSchedules.AddAsync(schedule, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return entry.Entity;
        }

        public async Task<WakeSchedule> UpdateWakeScheduleAsync(WakeSchedule schedule, CancellationToken cancellationToken = default) {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            _dbContext.WakeSchedules.Update(schedule);
            await SaveChangesAsync(cancellationToken);
            return schedule;
        }

        public async Task<bool> DeleteWakeScheduleAsync(int id, CancellationToken cancellationToken = default) {
            var schedule = await GetWakeScheduleByIdAsync(id, cancellationToken);
            if (schedule == null) {
                return false;
            }

            _dbContext.WakeSchedules.Remove(schedule);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

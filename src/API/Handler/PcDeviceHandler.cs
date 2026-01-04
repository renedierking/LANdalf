using API.Data;
using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;

namespace API.Handler {
    public class PcDeviceHandler {
        private readonly AppDbContext _db;
        private readonly WakeOnLanService _wolService;

        public PcDeviceHandler(AppDbContext db, WakeOnLanService wolService) {
            _db = db;
            _wolService = wolService;
        }

        public async Task<IResult> GetAllDevices(CancellationToken cancellationToken) {
            var pcs = await _db.PcDevices.ToListAsync(cancellationToken);
            return Results.Ok(pcs);
        }

        public async Task<IResult> GetDeviceById(int id, CancellationToken cancellationToken) {
            var pc = await _db.PcDevices.FindAsync(new object[] { id }, cancellationToken);
            if (pc == null) {
                return Results.NotFound();
            }
            return Results.Ok(pc);
        }

        public async Task<IResult> AddDevice(PcCreateDto dto, CancellationToken cancellationToken) {
            if (!PhysicalAddress.TryParse(dto.MacAddress, out var mac)) {
                return Results.BadRequest("MAC-Address invalid");
            }

            if (!IPAddress.TryParse(dto.IpAddress, out var ip)) {
                return Results.BadRequest("IP-Address invalid");
            }

            if (!IPAddress.TryParse(dto.BroadcastAddress, out var broadcast)) {
                return Results.BadRequest("Broadcast-Address invalid");
            }

            var pc = new PcDevice {
                Name = dto.Name,
                MacAddress = mac,
                IpAddress = ip,
                BroadcastAddress = broadcast
            };

            _db.PcDevices.Add(pc);
            await _db.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/pc-devices/{pc.Id}", pc);
        }

        public async Task<IResult> DeleteDevice(int id, CancellationToken cancellationToken) {
            var pc = await _db.PcDevices.FindAsync(new object[] { id }, cancellationToken);
            if (pc == null) {
                return Results.NotFound();
            }
            _db.PcDevices.Remove(pc);
            await _db.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        }

        public async Task<IResult> WakeDevice(int id, CancellationToken cancellationToken) {
            var pc = await _db.PcDevices.FindAsync(new object[] { id }, cancellationToken);
            if (pc == null) {
                return Results.NotFound();
            }
            await _wolService.Wake(pc.MacAddress, pc.BroadcastAddress);
            return Results.Ok(new { message = $"Wake-on-LAN packet sent to {pc.Name}" });
        }
    }
}

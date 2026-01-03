using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;

namespace API.Handler {
    public class PcDeviceHandler {
        public static async Task<IResult> GetAllDevices(AppDbContext db, CancellationToken cancellationToken) {
            var pcs = await db.PcDevices.ToListAsync(cancellationToken);

            return Results.Ok(pcs);
        }

        public static async Task<IResult> GetDeviceById(AppDbContext db, int id, CancellationToken cancellationToken) {
            var pc = await db.PcDevices.FindAsync([id], cancellationToken);
            if (pc == null) {
                return Results.NotFound();
            }
            return Results.Ok(pc);
        }

        public static async Task<IResult> AddDevice(AppDbContext db, PcCreateDto dto, CancellationToken cancellationToken) {
            if (!PhysicalAddress.TryParse(dto.MacAddress, out var mac))
                return Results.BadRequest("MAC-Address invalid");

            if (!IPAddress.TryParse(dto.IpAddress, out var ip))
                return Results.BadRequest("IP-Address invalid");

            if (!IPAddress.TryParse(dto.BroadcastAddress, out var broadcast))
                return Results.BadRequest("Broadcast-Address invalid");

            var pc = new PcDevice {
                Name = dto.Name,
                MacAddress = mac,
                IpAddress = ip,
                BroadcastAddress = broadcast
            };

            db.PcDevices.Add(pc);
            await db.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/pc-devices/{pc.Id}", pc);
        }

        public static async Task<IResult> DeleteDevice(AppDbContext db, int id, CancellationToken cancellationToken) {
            var pc = await db.PcDevices.FindAsync([id], cancellationToken);
            if (pc == null) {
                return Results.NotFound();
            }
            db.PcDevices.Remove(pc);
            await db.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        }

        public static async Task<IResult> WakeDevice(AppDbContext db, int id, Services.WakeOnLanService wolService, CancellationToken cancellationToken) {
            var pc = await db.PcDevices.FindAsync([id], cancellationToken);
            if (pc == null) {
                return Results.NotFound();
            }
            await wolService.Wake(pc.MacAddress, pc.BroadcastAddress);
            return Results.Ok(new { message = $"Wake-on-LAN packet sent to {pc.Name}" });
        }
    }
}

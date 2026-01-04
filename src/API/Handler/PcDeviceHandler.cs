using API.Data;
using API.DTOs;
using API.Models;
using API.Services;
using LANdalf.API.DTOs;
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
            var result = pcs.Select(pc => new PcDeviceDTO(
                Id: pc.Id,
                Name: pc.Name,
                MacAddress: pc.MacAddress.ToString(),
                IpAddress: pc.IpAddress?.ToString(),
                BroadcastAddress: pc.BroadcastAddress?.ToString(),
                IsOnline: pc.IsOnline
                ))
                .ToList();
            return Results.Ok(result);
        }

        public async Task<IResult> GetDeviceById(int id, CancellationToken cancellationToken) {
            var pc = await _db.PcDevices.FindAsync(new object[] { id }, cancellationToken);
            if (pc == null) {
                return Results.NotFound();
            }
            var result = new PcDeviceDTO(
                Id: pc.Id,
                Name: pc.Name,
                MacAddress: pc.MacAddress.ToString(),
                IpAddress: pc.IpAddress?.ToString(),
                BroadcastAddress: pc.BroadcastAddress?.ToString(),
                IsOnline: pc.IsOnline
            );

            return Results.Ok(result);
        }

        public async Task<IResult> AddDevice(PcCreateDto dto, CancellationToken cancellationToken) {
            if (!PhysicalAddress.TryParse(dto.MacAddress, out var mac)) {
                return Results.BadRequest("MAC-Address invalid");
            }

            IPAddress? ip = null;
            if (dto.IpAddress != null && !IPAddress.TryParse(dto.IpAddress, out ip)) {
                return Results.BadRequest("IP-Address invalid");
            }

            IPAddress? broadcast = null;
            if (dto.BroadcastAddress != null && !IPAddress.TryParse(dto.BroadcastAddress, out broadcast)) {
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

            var result = new PcDeviceDTO(
                Id: pc.Id,
                Name: pc.Name,
                MacAddress: pc.MacAddress.ToString(),
                IpAddress: pc.IpAddress?.ToString(),
                BroadcastAddress: pc.BroadcastAddress?.ToString(),
                IsOnline: pc.IsOnline
            );

            return Results.Created($"/api/pc-devices/{pc.Id}", result);
        }

        public async Task<IResult> SetDevice(int id, PcDeviceDTO dto, CancellationToken cancellationToken) {
            var pc = await _db.PcDevices.FindAsync(new object[] { id }, cancellationToken);
            if (pc == null) {
                return Results.NotFound();
            }
            if (!PhysicalAddress.TryParse(dto.MacAddress, out var mac)) {
                return Results.BadRequest("MAC-Address invalid");
            }
            IPAddress? ip = null;
            if (dto.IpAddress != null && !IPAddress.TryParse(dto.IpAddress, out ip)) {
                return Results.BadRequest("IP-Address invalid");
            }
            IPAddress? broadcast = null;
            if (dto.BroadcastAddress != null && !IPAddress.TryParse(dto.BroadcastAddress, out broadcast)) {
                return Results.BadRequest("Broadcast-Address invalid");
            }

            pc.Name = dto.Name;
            pc.MacAddress = mac;
            pc.IpAddress = ip;
            pc.BroadcastAddress = broadcast;
            await _db.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
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

using API.DTOs;
using API.Models;
using API.Services;
using LANdalf.API.DTOs;
using LANdalf.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.NetworkInformation;

namespace API.Handler {
    public class PcDeviceHandler {
        private readonly IAppDbService _appDbService;
        private readonly WakeOnLanService _wolService;
        private readonly ILogger<PcDeviceHandler> _logger;

        public PcDeviceHandler(IAppDbService appDbService, WakeOnLanService wolService, ILogger<PcDeviceHandler> logger) {
            _appDbService = appDbService ?? throw new ArgumentNullException(nameof(appDbService));
            _wolService = wolService ?? throw new ArgumentNullException(nameof(wolService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IResult> GetAllDevices(CancellationToken cancellationToken) {
            _logger.LogInformation("Retrieving all PC devices");
            var pcs = await _appDbService.GetAllPcDevicesAsync(cancellationToken);
            var result = pcs.Select(pc => pc.ToDto())
                .ToList();
            _logger.LogInformation("Retrieved {DeviceCount} PC devices", result.Count);
            return Results.Ok(result);
        }

        public async Task<IResult> GetDeviceById(int id, CancellationToken cancellationToken) {
            _logger.LogInformation("Retrieving PC device {DeviceId}", id);
            var pc = await _appDbService.GetPcDeviceByIdAsync(id, cancellationToken);
            if (pc == null) {
                _logger.LogWarning("PC device {DeviceId} not found", id);
                return CreateNotFoundResult($"PC device with ID {id} not found");
            }

            var result = pc.ToDto();
            return Results.Ok(result);
        }

        public async Task<IResult> AddDevice(PcCreateDto dto, CancellationToken cancellationToken) {
            _logger.LogInformation("Creating new PC device {DeviceName}", dto.Name);
            if (!PhysicalAddress.TryParse(dto.MacAddress, out var mac)) {
                _logger.LogWarning("Invalid MAC address provided: {MacAddress}", dto.MacAddress);
                return CreateBadRequestResult("MAC-Address invalid");
            }

            IPAddress? ip = null;
            if (dto.IpAddress != null && !IPAddress.TryParse(dto.IpAddress, out ip)) {
                _logger.LogWarning("Invalid IP address provided: {IpAddress}", dto.IpAddress);
                return CreateBadRequestResult("IP-Address invalid");
            }

            IPAddress? broadcast = null;
            if (dto.BroadcastAddress != null && !IPAddress.TryParse(dto.BroadcastAddress, out broadcast)) {
                _logger.LogWarning("Invalid broadcast address provided: {BroadcastAddress}", dto.BroadcastAddress);
                return CreateBadRequestResult("Broadcast-Address invalid");
            }

            var pc = new PcDevice {
                Name = dto.Name,
                MacAddress = mac,
                IpAddress = ip,
                BroadcastAddress = broadcast
            };

            var createdPc = await _appDbService.CreatePcDeviceAsync(pc, cancellationToken);
            _logger.LogInformation("Created PC device {DeviceId} ({DeviceName})", createdPc.Id, createdPc.Name);

            var result = createdPc.ToDto();
            return Results.Created($"/api/pc-devices/{createdPc.Id}", result);
        }

        public async Task<IResult> SetDevice(int id, PcDeviceDTO dto, CancellationToken cancellationToken) {
            _logger.LogInformation("Updating PC device {DeviceId}", id);
            var pc = await _appDbService.GetPcDeviceByIdAsync(id, cancellationToken);
            if (pc == null) {
                _logger.LogWarning("PC device {DeviceId} not found for update", id);
                return CreateNotFoundResult($"PC device with ID {id} not found");
            }
            if (!PhysicalAddress.TryParse(dto.MacAddress, out var mac)) {
                _logger.LogWarning("Invalid MAC address provided for device {DeviceId}: {MacAddress}", id, dto.MacAddress);
                return CreateBadRequestResult("MAC-Address invalid");
            }
            IPAddress? ip = null;
            if (dto.IpAddress != null && !IPAddress.TryParse(dto.IpAddress, out ip)) {
                _logger.LogWarning("Invalid IP address provided for device {DeviceId}: {IpAddress}", id, dto.IpAddress);
                return CreateBadRequestResult("IP-Address invalid");
            }
            IPAddress? broadcast = null;
            if (dto.BroadcastAddress != null && !IPAddress.TryParse(dto.BroadcastAddress, out broadcast)) {
                _logger.LogWarning("Invalid broadcast address provided for device {DeviceId}: {BroadcastAddress}", id, dto.BroadcastAddress);
                return CreateBadRequestResult("Broadcast-Address invalid");
            }

            pc.Name = dto.Name;
            pc.MacAddress = mac;
            pc.IpAddress = ip;
            pc.BroadcastAddress = broadcast;
            await _appDbService.UpdatePcDeviceAsync(pc, cancellationToken);
            _logger.LogInformation("Updated PC device {DeviceId} ({DeviceName})", id, dto.Name);

            return Results.NoContent();
        }

        public async Task<IResult> DeleteDevice(int id, CancellationToken cancellationToken) {
            _logger.LogInformation("Deleting PC device {DeviceId}", id);
            var deleted = await _appDbService.DeletePcDeviceAsync(id, cancellationToken);
            if (!deleted) {
                _logger.LogWarning("PC device {DeviceId} not found for deletion", id);
                return CreateNotFoundResult($"PC device with ID {id} not found");
            }
            _logger.LogInformation("Deleted PC device {DeviceId}", id);
            return Results.NoContent();
        }

        public async Task<IResult> WakeDevice(int id, CancellationToken cancellationToken) {
            _logger.LogInformation("Wake-on-LAN requested for device {DeviceId}", id);
            var pc = await _appDbService.GetPcDeviceByIdAsync(id, cancellationToken);
            if (pc == null) {
                _logger.LogWarning("PC device {DeviceId} not found for WoL", id);
                return CreateNotFoundResult($"PC device with ID {id} not found");
            }
            await _wolService.Wake(pc.MacAddress, pc.BroadcastAddress);
            _logger.LogInformation("Wake-on-LAN packet sent to device {DeviceId} ({DeviceName}, MAC: {MacAddress})", id, pc.Name, pc.MacAddress);
            return Results.Ok(new { message = $"Wake-on-LAN packet sent to {pc.Name}" });
        }

        private IResult CreateBadRequestResult(string detail) {
            var problemDetails = new ProblemDetails {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Detail = detail
            };
            return Results.BadRequest(problemDetails);
        }

        private IResult CreateNotFoundResult(string detail) {
            var problemDetails = new ProblemDetails {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "The specified resource was not found.",
                Status = StatusCodes.Status404NotFound,
                Detail = detail
            };
            return Results.NotFound(problemDetails);
        }
    }
}

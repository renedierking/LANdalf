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
            var pcs = await _appDbService.GetAllPcDevicesAsync(cancellationToken);
            var result = pcs.Select(pc => pc.ToDto())
                .ToList();
            return Results.Ok(result);
        }

        public async Task<IResult> GetDeviceById(int id, CancellationToken cancellationToken) {
            var pc = await _appDbService.GetPcDeviceByIdAsync(id, cancellationToken);
            if (pc == null) {
                return CreateNotFoundResult($"PC device with ID {id} not found");
            }

            var result = pc.ToDto();
            return Results.Ok(result);
        }

        public async Task<IResult> AddDevice(PcCreateDto dto, CancellationToken cancellationToken) {
            if (!PhysicalAddress.TryParse(dto.MacAddress, out var mac)) {
                return CreateBadRequestResult("MAC-Address invalid");
            }

            IPAddress? ip = null;
            if (dto.IpAddress != null && !IPAddress.TryParse(dto.IpAddress, out ip)) {
                return CreateBadRequestResult("IP-Address invalid");
            }

            IPAddress? broadcast = null;
            if (dto.BroadcastAddress != null && !IPAddress.TryParse(dto.BroadcastAddress, out broadcast)) {
                return CreateBadRequestResult("Broadcast-Address invalid");
            }

            var pc = new PcDevice {
                Name = dto.Name,
                MacAddress = mac,
                IpAddress = ip,
                BroadcastAddress = broadcast
            };

            var createdPc = await _appDbService.CreatePcDeviceAsync(pc, cancellationToken);

            var result = createdPc.ToDto();
            return Results.Created($"/api/pc-devices/{createdPc.Id}", result);
        }

        public async Task<IResult> SetDevice(int id, PcDeviceDTO dto, CancellationToken cancellationToken) {
            var pc = await _appDbService.GetPcDeviceByIdAsync(id, cancellationToken);
            if (pc == null) {
                return CreateNotFoundResult($"PC device with ID {id} not found");
            }
            if (!PhysicalAddress.TryParse(dto.MacAddress, out var mac)) {
                return CreateBadRequestResult("MAC-Address invalid");
            }
            IPAddress? ip = null;
            if (dto.IpAddress != null && !IPAddress.TryParse(dto.IpAddress, out ip)) {
                return CreateBadRequestResult("IP-Address invalid");
            }
            IPAddress? broadcast = null;
            if (dto.BroadcastAddress != null && !IPAddress.TryParse(dto.BroadcastAddress, out broadcast)) {
                return CreateBadRequestResult("Broadcast-Address invalid");
            }

            pc.Name = dto.Name;
            pc.MacAddress = mac;
            pc.IpAddress = ip;
            pc.BroadcastAddress = broadcast;
            await _appDbService.UpdatePcDeviceAsync(pc, cancellationToken);

            return Results.NoContent();
        }

        public async Task<IResult> DeleteDevice(int id, CancellationToken cancellationToken) {
            var deleted = await _appDbService.DeletePcDeviceAsync(id, cancellationToken);
            if (!deleted) {
                return CreateNotFoundResult($"PC device with ID {id} not found");
            }
            return Results.NoContent();
        }

        public async Task<IResult> WakeDevice(int id, CancellationToken cancellationToken) {
            var pc = await _appDbService.GetPcDeviceByIdAsync(id, cancellationToken);
            if (pc == null) {
                return CreateNotFoundResult($"PC device with ID {id} not found");
            }
            await _wolService.Wake(pc.MacAddress, pc.BroadcastAddress);
            _logger.LogDebug("Wake-on-LAN packet sent to device {DeviceId} ({DeviceName}, MAC: {MacAddress})", id, pc.Name, pc.MacAddress);
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

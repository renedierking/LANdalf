using API.Services;
using LANdalf.API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace API.Handler {
    public class DeviceEventHandler {
        private readonly IAppDbService _appDbService;
        private readonly ILogger<DeviceEventHandler> _logger;

        public DeviceEventHandler(IAppDbService appDbService, ILogger<DeviceEventHandler> logger) {
            _appDbService = appDbService ?? throw new ArgumentNullException(nameof(appDbService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IResult> GetAllEvents(CancellationToken cancellationToken) {
            var events = await _appDbService.GetAllDeviceEventsAsync(cancellationToken);
            var result = events.Select(e => e.ToDto()).ToList();
            return Results.Ok(result);
        }

        public async Task<IResult> GetEventsByDeviceId(int deviceId, int limit, int offset, CancellationToken cancellationToken) {
            var events = await _appDbService.GetDeviceEventsByDeviceIdAsync(deviceId, limit, offset, cancellationToken);
            var result = events.Select(e => e.ToDto()).ToList();
            return Results.Ok(result);
        }

        public async Task<IResult> GetEventById(int id, CancellationToken cancellationToken) {
            var deviceEvent = await _appDbService.GetDeviceEventByIdAsync(id, cancellationToken);
            if (deviceEvent == null) {
                return CreateNotFoundResult($"Device event with ID {id} not found");
            }

            var result = deviceEvent.ToDto();
            return Results.Ok(result);
        }

        public async Task<IResult> DeleteEvent(int id, CancellationToken cancellationToken) {
            var deleted = await _appDbService.DeleteDeviceEventAsync(id, cancellationToken);
            if (!deleted) {
                return CreateNotFoundResult($"Device event with ID {id} not found");
            }

            _logger.LogInformation("Deleted device event {EventId}", id);
            return Results.NoContent();
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

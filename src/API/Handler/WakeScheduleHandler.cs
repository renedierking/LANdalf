using API.DTOs;
using API.Models;
using API.Services;
using LANdalf.API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace API.Handler {
    public class WakeScheduleHandler {
        private readonly IAppDbService _appDbService;
        private readonly ILogger<WakeScheduleHandler> _logger;

        public WakeScheduleHandler(IAppDbService appDbService, ILogger<WakeScheduleHandler> logger) {
            _appDbService = appDbService ?? throw new ArgumentNullException(nameof(appDbService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IResult> GetAllSchedules(CancellationToken cancellationToken) {
            var schedules = await _appDbService.GetAllWakeSchedulesAsync(cancellationToken);
            var result = schedules.Select(s => s.ToDto()).ToList();
            return Results.Ok(result);
        }

        public async Task<IResult> GetSchedulesByDeviceId(int deviceId, CancellationToken cancellationToken) {
            var schedules = await _appDbService.GetWakeSchedulesByDeviceIdAsync(deviceId, cancellationToken);
            var result = schedules.Select(s => s.ToDto()).ToList();
            return Results.Ok(result);
        }

        public async Task<IResult> GetScheduleById(int id, CancellationToken cancellationToken) {
            var schedule = await _appDbService.GetWakeScheduleByIdAsync(id, cancellationToken);
            if (schedule == null) {
                return CreateNotFoundResult($"Wake schedule with ID {id} not found");
            }

            var result = schedule.ToDto();
            return Results.Ok(result);
        }

        public async Task<IResult> AddSchedule(WakeScheduleCreateDto dto, CancellationToken cancellationToken) {
            // Validate device exists
            var device = await _appDbService.GetPcDeviceByIdAsync(dto.PcDeviceId, cancellationToken);
            if (device == null) {
                return CreateNotFoundResult($"Device with ID {dto.PcDeviceId} not found");
            }

            // Parse scheduled time
            if (!TimeOnly.TryParse(dto.ScheduledTime, out var scheduledTime)) {
                return CreateBadRequestResult("Invalid time format. Expected HH:mm");
            }

            // Validate days of week if provided
            if (!string.IsNullOrWhiteSpace(dto.DaysOfWeek)) {
                if (!ValidateDaysOfWeek(dto.DaysOfWeek)) {
                    return CreateBadRequestResult("Invalid DaysOfWeek format. Expected comma-separated values 0-6 (0=Sunday)");
                }
            }

            var schedule = new WakeSchedule {
                PcDeviceId = dto.PcDeviceId,
                ScheduledTime = scheduledTime,
                DaysOfWeek = string.IsNullOrWhiteSpace(dto.DaysOfWeek) ? null : dto.DaysOfWeek.Trim(),
                Enabled = dto.Enabled
            };

            // Generate cron expression and calculate next execution
            var utcNow = DateTime.UtcNow;
            WakeScheduleHelper.UpdateScheduleExecution(schedule, utcNow);

            var createdSchedule = await _appDbService.CreateWakeScheduleAsync(schedule, cancellationToken);

            // Load device for DTO
            createdSchedule.PcDevice = device;

            var result = createdSchedule.ToDto();
            _logger.LogInformation("Created wake schedule {ScheduleId} for device {DeviceName} (ID={DeviceId})",
                createdSchedule.Id, device.Name, device.Id);

            return Results.Created($"/api/v1/wake-schedules/{createdSchedule.Id}", result);
        }

        public async Task<IResult> UpdateSchedule(int id, WakeScheduleCreateDto dto, CancellationToken cancellationToken) {
            var schedule = await _appDbService.GetWakeScheduleByIdAsync(id, cancellationToken);
            if (schedule == null) {
                return CreateNotFoundResult($"Wake schedule with ID {id} not found");
            }

            // Validate device exists
            var device = await _appDbService.GetPcDeviceByIdAsync(dto.PcDeviceId, cancellationToken);
            if (device == null) {
                return CreateNotFoundResult($"Device with ID {dto.PcDeviceId} not found");
            }

            // Parse scheduled time
            if (!TimeOnly.TryParse(dto.ScheduledTime, out var scheduledTime)) {
                return CreateBadRequestResult("Invalid time format. Expected HH:mm");
            }

            // Validate days of week if provided
            if (!string.IsNullOrWhiteSpace(dto.DaysOfWeek)) {
                if (!ValidateDaysOfWeek(dto.DaysOfWeek)) {
                    return CreateBadRequestResult("Invalid DaysOfWeek format. Expected comma-separated values 0-6 (0=Sunday)");
                }
            }

            schedule.PcDeviceId = dto.PcDeviceId;
            schedule.ScheduledTime = scheduledTime;
            schedule.DaysOfWeek = string.IsNullOrWhiteSpace(dto.DaysOfWeek) ? null : dto.DaysOfWeek.Trim();
            schedule.Enabled = dto.Enabled;

            // Regenerate cron expression and recalculate next execution
            var utcNow = DateTime.UtcNow;
            WakeScheduleHelper.UpdateScheduleExecution(schedule, utcNow);

            await _appDbService.UpdateWakeScheduleAsync(schedule, cancellationToken);

            _logger.LogInformation("Updated wake schedule {ScheduleId}", id);
            return Results.NoContent();
        }

        public async Task<IResult> DeleteSchedule(int id, CancellationToken cancellationToken) {
            var deleted = await _appDbService.DeleteWakeScheduleAsync(id, cancellationToken);
            if (!deleted) {
                return CreateNotFoundResult($"Wake schedule with ID {id} not found");
            }

            _logger.LogInformation("Deleted wake schedule {ScheduleId}", id);
            return Results.NoContent();
        }

        public async Task<IResult> ToggleSchedule(int id, CancellationToken cancellationToken) {
            var schedule = await _appDbService.GetWakeScheduleByIdAsync(id, cancellationToken);
            if (schedule == null) {
                return CreateNotFoundResult($"Wake schedule with ID {id} not found");
            }

            schedule.Enabled = !schedule.Enabled;

            // Update next execution based on new enabled state
            var utcNow = DateTime.UtcNow;
            WakeScheduleHelper.UpdateScheduleExecution(schedule, utcNow);

            await _appDbService.UpdateWakeScheduleAsync(schedule, cancellationToken);

            _logger.LogInformation("Toggled wake schedule {ScheduleId} to {Enabled}", id, schedule.Enabled);
            return Results.Ok(new { enabled = schedule.Enabled });
        }

        private bool ValidateDaysOfWeek(string daysOfWeek) {
            var parts = daysOfWeek.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts) {
                if (!int.TryParse(part, out var day) || day < 0 || day > 6) {
                    return false;
                }
            }
            return parts.Length > 0;
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

using API.DTOs;
using API.Handler;
using System.Net;

namespace API.MinimalApi {
    public class WakeScheduleMinimalApiStrategy : IMinimalApiStrategy {
        public void MapEndpoints(RouteGroupBuilder group) {
            group.MapGet("/wake-schedules/", (WakeScheduleHandler handler, CancellationToken ct) => handler.GetAllSchedules(ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetAllWakeSchedules";
                    operation.Summary = "Get all wake schedules.";
                    operation.Description = "Returns an array of wake schedules for all devices.";
                    return Task.CompletedTask;
                })
                .Produces<IReadOnlyList<WakeScheduleDTO>>();

            group.MapGet("/pc-devices/{deviceId}/wake-schedules/", (WakeScheduleHandler handler, int deviceId, CancellationToken ct) => handler.GetSchedulesByDeviceId(deviceId, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetWakeSchedulesByDevice";
                    operation.Summary = "Get wake schedules for a specific device.";
                    operation.Description = "Returns an array of wake schedules for the specified device.";
                    return Task.CompletedTask;
                })
                .Produces<IReadOnlyList<WakeScheduleDTO>>();

            group.MapGet("/wake-schedules/{id}", (WakeScheduleHandler handler, int id, CancellationToken ct) => handler.GetScheduleById(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetWakeScheduleById";
                    operation.Summary = "Get a specific wake schedule.";
                    operation.Description = "Returns a wake schedule by ID.";
                    return Task.CompletedTask;
                })
                .Produces<WakeScheduleDTO>()
                .ProducesProblem((int)HttpStatusCode.NotFound);

            group.MapPost("/wake-schedules/add", (WakeScheduleHandler handler, WakeScheduleCreateDto dto, CancellationToken ct) => handler.AddSchedule(dto, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "AddWakeSchedule";
                    operation.Summary = "Add a new wake schedule.";
                    operation.Description = "Creates a new wake schedule for a device.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.Created)
                .ProducesProblem((int)HttpStatusCode.BadRequest)
                .ProducesProblem((int)HttpStatusCode.NotFound);

            group.MapPost("/wake-schedules/{id}/set", (WakeScheduleHandler handler, int id, WakeScheduleCreateDto dto, CancellationToken ct) => handler.UpdateSchedule(id, dto, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "UpdateWakeSchedule";
                    operation.Summary = "Updates an existing wake schedule.";
                    operation.Description = "Updates the specified wake schedule.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.NoContent)
                .ProducesProblem((int)HttpStatusCode.NotFound)
                .ProducesProblem((int)HttpStatusCode.BadRequest);

            group.MapPost("/wake-schedules/{id}/delete", (WakeScheduleHandler handler, int id, CancellationToken ct) => handler.DeleteSchedule(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "DeleteWakeSchedule";
                    operation.Summary = "Deletes a wake schedule.";
                    operation.Description = "Deletes the specified wake schedule.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.NoContent)
                .ProducesProblem((int)HttpStatusCode.NotFound);

            group.MapPost("/wake-schedules/{id}/toggle", (WakeScheduleHandler handler, int id, CancellationToken ct) => handler.ToggleSchedule(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "ToggleWakeSchedule";
                    operation.Summary = "Toggle a wake schedule's enabled state.";
                    operation.Description = "Enables or disables the specified wake schedule.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.OK)
                .ProducesProblem((int)HttpStatusCode.NotFound);
        }
    }
}

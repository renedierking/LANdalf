using API.Handler;
using LANdalf.API.DTOs;
using System.Net;

namespace API.MinimalApi {
    public class DeviceEventMinimalApiStrategy : IMinimalApiStrategy {
        public void MapEndpoints(RouteGroupBuilder group) {
            group.MapGet("/device-events/", (DeviceEventHandler handler, CancellationToken ct) => handler.GetAllEvents(ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetAllDeviceEvents";
                    operation.Summary = "Get all device events.";
                    operation.Description = "Returns an array of all device events ordered by timestamp (newest first).";
                    return Task.CompletedTask;
                })
                .Produces<IReadOnlyList<DeviceEventDTO>>();

            group.MapGet("/pc-devices/{deviceId}/history", (DeviceEventHandler handler, int deviceId, int limit = 50, int offset = 0, CancellationToken ct = default) => handler.GetEventsByDeviceId(deviceId, limit, offset, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetDeviceHistory";
                    operation.Summary = "Get event history for a specific device.";
                    operation.Description = "Returns paginated event history for the specified device, ordered by timestamp (newest first).";
                    return Task.CompletedTask;
                })
                .Produces<IReadOnlyList<DeviceEventDTO>>();

            group.MapGet("/device-events/{id}", (DeviceEventHandler handler, int id, CancellationToken ct) => handler.GetEventById(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetDeviceEventById";
                    operation.Summary = "Get a specific device event.";
                    operation.Description = "Returns a device event by ID.";
                    return Task.CompletedTask;
                })
                .Produces<DeviceEventDTO>()
                .ProducesProblem((int)HttpStatusCode.NotFound);

            group.MapPost("/device-events/{id}/delete", (DeviceEventHandler handler, int id, CancellationToken ct) => handler.DeleteEvent(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "DeleteDeviceEvent";
                    operation.Summary = "Deletes a device event.";
                    operation.Description = "Deletes the specified device event.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.NoContent)
                .ProducesProblem((int)HttpStatusCode.NotFound);
        }
    }
}

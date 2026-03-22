using API.DTOs;
using API.Handler;
using LANdalf.API.DTOs;
using System.Net;

namespace API.MinimalApi {
    public class PcDeviceMinimalApiStrategy : IMinimalApiStrategy {
        public void MapEndpoints(RouteGroupBuilder group) {
            group.MapGet("/pc-devices/", (PcDeviceHandler handler, CancellationToken ct) => handler.GetAllDevices(ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetAllPcDevices";
                    operation.Summary = "Get all PcDevices.";
                    operation.Description = "Returns an array of PcDevices.";
                    return Task.CompletedTask;
                }).Produces<IReadOnlyList<PcDeviceDTO>>();

            group.MapGet("/pc-devices/{id}", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.GetDeviceById(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetPcDeviceById";
                    operation.Summary = "Get a specific PcDevice.";
                    operation.Description = "Returns a PcDevice.";
                    return Task.CompletedTask;
                }).Produces<PcDeviceDTO>()
                .ProducesProblem((int)HttpStatusCode.NotFound);

            group.MapPost("/pc-devices/add", (PcDeviceHandler handler, PcCreateDto dto, CancellationToken ct) => handler.AddDevice(dto, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "AddPcDevice";
                    operation.Summary = "Add a new PcDevice.";
                    operation.Description = "Returns a Created-Result.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.Created)
                .ProducesProblem((int)HttpStatusCode.BadRequest);

            group.MapPost("/pc-devices/{id}/set", (PcDeviceHandler handler, int id, PcDeviceDTO dto, CancellationToken ct) => handler.SetDevice(id, dto, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "SetPcDevice";
                    operation.Summary = "Updates an existing PcDevice.";
                    operation.Description = "Returns a NoContent-Result.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.NoContent)
                .ProducesProblem((int)HttpStatusCode.NotFound)
                .ProducesProblem((int)HttpStatusCode.BadRequest);

            group.MapPost("/pc-devices/{id}/delete", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.DeleteDevice(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "DeletePcDevice";
                    operation.Summary = "Deletes an existing PcDevice.";
                    operation.Description = "Returns a NoContent-Result.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.NoContent)
                .ProducesProblem((int)HttpStatusCode.NotFound);

            group.MapPost("/pc-devices/{id}/wake", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.WakeDevice(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "WakePcDevice";
                    operation.Summary = "Wakes a PcDevice.";
                    operation.Description = "Returns an Ok-Result.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.OK)
                .ProducesProblem((int)HttpStatusCode.NotFound);
        }
    }
}

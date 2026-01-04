using API.Data;
using API.DTOs;
using API.Handler;
using API.Models;
using API.Services;
using Asp.Versioning;
using LANdalf.API.DTOs;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Net;

namespace API {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            var dbPath = Path.Combine(builder.Environment.ContentRootPath, "LANdalf", "landalf.db");
            var dbDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDir)) {
                Directory.CreateDirectory(dbDir);
            }

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(o =>
                o.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddScoped<WakeOnLanService>();
            builder.Services.AddScoped<PcDeviceHandler>();

            builder.Services.AddApiVersioning(options => {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            }).AddApiExplorer(options => {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            using (var scope = app.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            //app.UseHttpsRedirection();
            //app.UseAuthorization();
            //app.MapControllers();

            // Minimal API endpoints v1
            var versionedApi = app.NewVersionedApi("LANdalf-api");
            var v1 = versionedApi.MapGroup("/api/v{version:apiVersion}");
            v1.HasApiVersion(1.0);

            v1.MapGet("/pc-devices/", (PcDeviceHandler handler, CancellationToken ct) => handler.GetAllDevices(ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetAllPcDevices";
                    operation.Summary = "Get all PcDevices.";
                    operation.Description = "Returns an array of PcDevices.";
                    return Task.CompletedTask;
                }).Produces<IReadOnlyList<PcDeviceDTO>>();

            v1.MapGet("/pc-devices/{id}", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.GetDeviceById(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "GetPcDeviceById";
                    operation.Summary = "Get a specific PcDevices.";
                    operation.Description = "Returns a PcDevice.";
                    return Task.CompletedTask;
                }).Produces<PcDeviceDTO>()
                .ProducesProblem((int)HttpStatusCode.NotFound);

            v1.MapPost("/pc-devices/add", (PcDeviceHandler handler, PcCreateDto dto, CancellationToken ct) => handler.AddDevice(dto, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "AddPcDevice";
                    operation.Summary = "Add a new PcDevice.";
                    operation.Description = "Returns a Created-Result.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.Created)
                .ProducesProblem((int)HttpStatusCode.BadRequest);

            v1.MapPost("/pc-devices/{id}/set", (PcDeviceHandler handler, int id, PcDeviceDTO dto, CancellationToken ct) => handler.SetDevice(id, dto, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "SetPcDevice";
                    operation.Summary = "Updates an existing PcDevice.";
                    operation.Description = "Returns a NoContent-Result.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.NoContent)
                .ProducesProblem((int)HttpStatusCode.NotFound)
                .ProducesProblem((int)HttpStatusCode.BadRequest);


            v1.MapPost("/pc-devices/{id}/delete", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.DeleteDevice(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "DeletePcDevice";
                    operation.Summary = "Deletes an existing PcDevice.";
                    operation.Description = "Returns a NoContent-Result.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.NoContent)
                .ProducesProblem((int)HttpStatusCode.NotFound);

            v1.MapPost("/pc-devices/{id}/wake", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.WakeDevice(id, ct))
                .AddOpenApiOperationTransformer((operation, context, ct) => {
                    operation.OperationId = "WakePcDevice";
                    operation.Summary = "Wakes an PcDevice.";
                    operation.Description = "Returns a Ok-Result.";
                    return Task.CompletedTask;
                })
                .Produces((int)HttpStatusCode.OK)
                .ProducesProblem((int)HttpStatusCode.NotFound);

            app.Run();
        }
    }
}

using API.Data;
using API.DTOs;
using API.Handler;
using API.Services;
using Asp.Versioning;
using LANdalf.API.DTOs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace API {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            var dbPath = Path.Combine(builder.Environment.ContentRootPath, "LANdalf_Data", "landalf.db");
            var dbDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDir)) {
                Directory.CreateDirectory(dbDir);
            }

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(o =>
                o.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddScoped<IAppDbService, AppDbService>();
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

            string corsPolicyName = "AllowFrontend";
            builder.Services.AddCors(options => {
                var frontendUrl = builder.Configuration["Cors:FrontendUrl"] ?? "https://localhost:7052";
                options.AddPolicy(corsPolicyName, policy => {
                    policy.WithOrigins(frontendUrl)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });


            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            using (var scope = app.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            app.UseCors(corsPolicyName);

            // Global Exception-Handling for all Minimal-API-Endpoints
            app.UseExceptionHandler(exceptionApp => {
                exceptionApp.Run(async context => {
                    ILogger<Program> logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    IExceptionHandlerFeature? feature = context.Features.Get<IExceptionHandlerFeature>();
                    Exception? ex = feature?.Error;

                    string traceId = Activity.Current?.Id ?? context.TraceIdentifier;

                    int statusCode = StatusCodes.Status500InternalServerError;
                    string title = "An unhandled Error occured.";

                    if (ex is ArgumentException) {
                        statusCode = StatusCodes.Status400BadRequest;
                        title = "Invalid Request.";
                    } else if (ex is KeyNotFoundException) {
                        statusCode = StatusCodes.Status404NotFound;
                        title = "Resource not found.";
                    } else if (ex is UnauthorizedAccessException) {
                        statusCode = StatusCodes.Status401Unauthorized;
                        title = "Unauthorized.";
                    } else if (ex is OperationCanceledException) {
                        statusCode = StatusCodes.Status400BadRequest;
                        title = "Request canceled.";
                    }

                    ProblemDetailsFactory factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    ProblemDetails problem = factory.CreateProblemDetails(
                        httpContext: context,
                        statusCode: statusCode,
                        title: title,
                        type: null,
                        detail: ex?.Message,
                        instance: context.Request.Path);

                    problem.Extensions["traceId"] = traceId;

                    logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/problem+json";

                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    await JsonSerializer.SerializeAsync(context.Response.Body, problem, problem.GetType(), options);
                });
            });

            //app.UseStaticFiles(); // Vor app.UseRouting()

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

using API.Data;
using API.Handler;
using API.MinimalApi;
using API.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace API {
    public class Program {
        public static int Main(string[] args) {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

                var dbPath = Path.Combine(builder.Environment.ContentRootPath, "LANdalf_Data", "landalf.db");
                var dbDir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dbDir)) {
                    Directory.CreateDirectory(dbDir);
                }

                Log.Information("Database path: {DbPath}", dbPath);

                // Add services to the container.
                builder.Services.AddDbContext<AppDbContext>(o =>
                    o.UseSqlite($"Data Source={dbPath}"));

                builder.Services.AddScoped<IAppDbService, AppDbService>();
                builder.Services.AddScoped<WakeOnLanService>();
                builder.Services.AddScoped<PcDeviceHandler>();
                builder.Services.AddSingleton<IDeviceMonitoringService, DeviceMonitoringService>();
                builder.Services.AddHostedService(sp => (DeviceMonitoringService)sp.GetRequiredService<IDeviceMonitoringService>());
                builder.Services.AddMinimalApiStrategies();

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
                var frontendUrl = builder.Configuration["Cors:FrontendUrl"] ?? "https://localhost:7052";
                builder.Services.AddCors(options => {
                    options.AddPolicy(corsPolicyName, policy => {
                        policy.WithOrigins(frontendUrl)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
                });

                Log.Information("CORS allowed origin: {FrontendUrl}", frontendUrl);


                var app = builder.Build();

                if (app.Environment.IsDevelopment()) {
                    app.MapOpenApi();
                    app.MapScalarApiReference();
                }

                using (var scope = app.Services.CreateScope()) {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    Log.Information("Applying database migrations...");
                    db.Database.Migrate();
                    Log.Information("Database migrations applied successfully");
                }

                app.UseCors(corsPolicyName);
                app.UseSerilogRequestLogging();

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

                // Minimal API endpoints v1
                var versionedApi = app.NewVersionedApi("LANdalf-api");
                var v1 = versionedApi.MapGroup("/api/v{version:apiVersion}");
                v1.HasApiVersion(1.0);

                var minimalApiStrategies = app.Services.GetRequiredService<IEnumerable<IMinimalApiStrategy>>();
                v1.MapMinimalApiStrategies(minimalApiStrategies);

                Log.Information("LANdalf API started successfully");
                app.Run();

                return 0;
            } catch (Exception ex) {
                Log.Fatal(ex, "LANdalf API terminated unexpectedly");
                return 1;
            } finally {
                Log.CloseAndFlush();
            }
        }
    }
}

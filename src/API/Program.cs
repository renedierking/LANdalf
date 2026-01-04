using API.Data;
using API.DTOs;
using API.Handler;
using API.Services;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

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

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // Minimal API endpoints v1
            var versionedApi = app.NewVersionedApi("LANdalf-api");
            var v1 = versionedApi.MapGroup("/api/v{version:apiVersion}");
            v1.HasApiVersion(1.0);
                        
            v1.MapGet("/pc-devices/", (PcDeviceHandler handler, CancellationToken ct) => handler.GetAllDevices(ct));
            v1.MapGet("/pc-devices/{id}", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.GetDeviceById(id, ct));
            v1.MapPost("/pc-devices/add", (PcDeviceHandler handler, PcCreateDto dto, CancellationToken ct) => handler.AddDevice(dto, ct));
            v1.MapPost("/pc-devices/{id}/delete", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.DeleteDevice(id, ct));
            v1.MapPost("/pc-devices/{id}/wake", (PcDeviceHandler handler, int id, CancellationToken ct) => handler.WakeDevice(id, ct));

            app.Run();
        }
    }
}

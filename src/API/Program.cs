using API.Data;
using API.Handler;
using API.Services;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace API {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<AppDbContext>(o =>
                o.UseSqlite("Data Source=wol.db"));

            builder.Services.AddScoped<WakeOnLanService>();

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
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            // 🔽 DB-Initialisierung
            using (var scope = app.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            var versionedApi = app.NewVersionedApi("LANdalf-api");

            var v1 = versionedApi.MapGroup("/api/v{version:apiVersion}");
            v1.HasApiVersion(1.0);

            v1.MapGet("/pc-devices/", PcDeviceHandler.GetAllDevices);

            v1.MapPost("/pc-devices/add", PcDeviceHandler.AddDevice);

            v1.MapPost("/pc-devices/{id}/delete", PcDeviceHandler.DeleteDevice);

            v1.MapPost("/pc-devices{id}/wake", PcDeviceHandler.WakeDevice);

            //var v2 = versionedApi.MapGroup("/api/v{version:apiVersion}");
            //v2.HasApiVersion(2.0);

            app.Run();
        }
    }
}

using LANdalf.UI.ApiClient;
using LANdalf.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace LANdalf.UI {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddMudServices();
            builder.Services.AddScoped<ThemeService>();
            builder.Services.AddScoped<ViewPreferenceService>();

            var apiBase = builder.Configuration["ApiBaseAddress"] ?? builder.HostEnvironment.BaseAddress;

            // Store API URL in configuration for SignalR hub service
            builder.Configuration["ApiUrl"] = apiBase;

            builder.Services.AddHttpClient("LANdalf.Api", client => {
#if DEBUG
                client.BaseAddress = new Uri(apiBase);
#else
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
#endif
            });

            // Standard HttpClient (so: @inject HttpClient Http) uses the API
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("LANdalf.Api"));
            builder.Services.AddScoped(sp => new LANdalfApiClient(sp.GetRequiredService<HttpClient>()));

            builder.Services.AddScoped<LANdalfApiService>();
            builder.Services.AddScoped<IDeviceValidationService, DeviceValidationService>();
            builder.Services.AddSingleton<DeviceStatusHubService>();

            await builder.Build().RunAsync();
        }
    }
}

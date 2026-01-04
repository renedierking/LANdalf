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

            var apiBase = builder.Configuration["ApiBaseAddress"] ?? builder.HostEnvironment.BaseAddress;

            builder.Services.AddHttpClient("LANdalf.Api", client => {
                client.BaseAddress = new Uri(apiBase);
            });

            // Standard HttpClient (so: @inject HttpClient Http) nutzt die API
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("LANdalf.Api"));
            builder.Services.AddScoped(sp => new LANdalfApiClient(sp.GetRequiredService<HttpClient>()));

            builder.Services.AddScoped<LANdalfApiService>();

            await builder.Build().RunAsync();
        }
    }
}

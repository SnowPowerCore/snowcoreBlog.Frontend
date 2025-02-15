using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using snowcoreBlog.Frontend.ClientShared.Extensions;

namespace snowcoreBlog.Frontend.Client;

public class Program
{
    private static Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        });
        builder.Services.AddClient();

        return builder.Build().RunAsync();
    }
}
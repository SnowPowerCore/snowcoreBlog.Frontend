using BitzArt.Blazor.Auth.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using snowcoreBlog.Frontend.ClientShared.Extensions;
using snowcoreBlog.Frontend.ClientShared.Handlers;

namespace snowcoreBlog.Frontend.Client;

public class Program
{
    private static Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        }));
        builder.Services
            .AddHttpClient(string.Empty, sp => sp.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            .AddHttpMessageHandler<IncludeCookiesHandler>();
        builder.Services.AddClient();
        builder.AddBlazorAuth();

        return builder.Build().RunAsync();
    }
}
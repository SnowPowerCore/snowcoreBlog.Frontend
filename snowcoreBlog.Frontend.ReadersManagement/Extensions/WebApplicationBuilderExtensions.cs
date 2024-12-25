using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ReadersManagement.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebAssemblyHostBuilder AddReadersManagement(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddTimeWarpState();
        return builder;
    }
}
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace snowcoreBlog.Frontend.SharedComponents.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebAssemblyHostBuilder AddSharedComponents(this WebAssemblyHostBuilder builder)
    {
        return builder;
    }
}
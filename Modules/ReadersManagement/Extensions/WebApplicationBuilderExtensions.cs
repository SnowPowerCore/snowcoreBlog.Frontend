using MediatR.Pipeline;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Features.Persistence;
using TimeWarp.State.Plus;
using TimeWarp.State.Plus.Extensions;

namespace snowcoreBlog.Frontend.ReadersManagement.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebAssemblyHostBuilder AddReadersManagement(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
        builder.Services.AddScoped<IPersistenceService, PersistenceService>();
        builder.Services.AddTimeWarpStateRouting();
        return builder;
    }
}
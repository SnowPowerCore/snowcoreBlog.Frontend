using MediatR.Pipeline;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Features.Persistence;
using TimeWarp.State;
using TimeWarp.State.Plus;

namespace snowcoreBlog.Frontend.ReadersManagement.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebAssemblyHostBuilder AddReadersManagement(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddTimeWarpState();
        builder.Services.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
        builder.Services.AddScoped<IPersistenceService, PersistenceService>();
        return builder;
    }
}
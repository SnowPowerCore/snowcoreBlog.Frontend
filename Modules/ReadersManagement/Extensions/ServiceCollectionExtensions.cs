using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using snowcoreBlog.Frontend.Core.Interfaces.Antiforgery;
using snowcoreBlog.Frontend.ReadersManagement.Features.Antiforgery;
using TimeWarp.State.Plus;
using TimeWarp.State.Plus.Extensions;

namespace snowcoreBlog.Frontend.ReadersManagement.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReadersManagement(this IServiceCollection serviceCollection, IConfiguration? configuration = null)
    {
        serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
        serviceCollection.AddScoped<IPersistenceService, PersistenceService>();
        serviceCollection.AddTimeWarpStateRouting();

        serviceCollection.AddSingleton<IAntiforgeryTokenRefresher, ReadersManagementAntiforgeryTokenRefresher>();

        // ActualLab.Fusion: real-time nickname availability replica (no-op safe if configuration is null).
        if (configuration is not null)
            serviceCollection.AddFusionClient(configuration);

        return serviceCollection;
    }
}
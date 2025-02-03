using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.State.Plus;
using TimeWarp.State.Plus.Extensions;

namespace snowcoreBlog.Frontend.ReadersManagement.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReadersManagement(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
        serviceCollection.AddScoped<IPersistenceService, PersistenceService>();
        serviceCollection.AddTimeWarpStateRouting();
        return serviceCollection;
    }
}
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Blazor.Masonry.Registrars;
using TimeWarp.State.Plus;
using TimeWarp.State.Plus.Extensions;

namespace snowcoreBlog.Frontend.Articles.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArticles(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
        serviceCollection.AddScoped<IPersistenceService, PersistenceService>();
        serviceCollection.AddTimeWarpStateRouting();
        serviceCollection.AddMasonryInteropAsScoped();
        return serviceCollection;
    }
}
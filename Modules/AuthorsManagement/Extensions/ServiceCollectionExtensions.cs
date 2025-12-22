using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using snowcoreBlog.Frontend.AuthorsManagement.Components.Fluent;
using snowcoreBlog.Frontend.SharedComponents.LayoutHooks;
using TimeWarp.State.Plus;
using TimeWarp.State.Plus.Extensions;

namespace snowcoreBlog.Frontend.AuthorsManagement.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorsManagement(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
        serviceCollection.AddScoped<IPersistenceService, PersistenceService>();
        serviceCollection.AddTimeWarpStateRouting();

        serviceCollection.AddSingleton<ILayoutHook>(new LayoutHook(Order: 100, ComponentType: typeof(BecomeAuthorBanner)));

        return serviceCollection;
    }
}
using Microsoft.Extensions.DependencyInjection;
using snowcoreBlog.Frontend.SharedComponents.Services;

namespace snowcoreBlog.Frontend.SharedComponents.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedComponents(this IServiceCollection serviceCollection)
    {
        // Global overlay service for rendering modal content at root (used by GlobalOverlay)
        serviceCollection.AddSingleton<OverlayService>();

        // Theme preference state; persistence is owned by FluentDesignTheme (StorageName="theme")
        serviceCollection.AddScoped<ThemeService>();
        return serviceCollection;
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace snowcoreBlog.Frontend.Notifications.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection serviceCollection)
    {
        // Services for notifications will be registered here as needed
        // The TimeWarp state is automatically discovered from the assembly
        return serviceCollection;
    }
}

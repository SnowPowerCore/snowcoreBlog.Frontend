using Microsoft.Extensions.DependencyInjection;

namespace snowcoreBlog.Frontend.ServiceNotifications.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceNotifications(this IServiceCollection serviceCollection)
    {
        // Services for notifications will be registered here as needed
        // The TimeWarp state is automatically discovered from the assembly
        return serviceCollection;
    }
}

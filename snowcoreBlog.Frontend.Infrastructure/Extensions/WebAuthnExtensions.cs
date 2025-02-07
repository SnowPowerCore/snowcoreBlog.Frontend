using Microsoft.Extensions.DependencyInjection;
using snowcoreBlog.Frontend.Infrastructure.Protocol;

namespace snowcoreBlog.Frontend.Infrastructure.Extensions;

public static class WebAuthnExtensions
{
    /// <summary>
    /// Adds the <see cref="WebAuthn"/> service to the DI container.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddWebAuthn(this IServiceCollection services) =>
        services.AddSingleton<WebAuthn>();
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using snowcoreBlog.Frontend.Infrastructure.Antiforgery;

namespace snowcoreBlog.Frontend.Infrastructure.Extensions;

public static class AntiforgeryRetryHandlerExtensions
{
    /// <summary>
    /// Registers <see cref="AntiforgeryRetryHandler"/> and adds it as the
    /// outermost message handler of the client.
    /// </summary>
    public static IHttpClientBuilder AddAntiforgeryRetryHandler(this IHttpClientBuilder builder)
    {
        builder.Services.TryAddTransient<AntiforgeryRetryHandler>();
        return builder.AddHttpMessageHandler<AntiforgeryRetryHandler>();
    }
}
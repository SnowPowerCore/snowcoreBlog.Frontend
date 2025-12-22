using Refit;
using snowcoreBlog.Frontend.Host.Handlers;
using snowcoreBlog.Frontend.Infrastructure.Context;
using snowcoreBlog.PublicApi.Api;

namespace snowcoreBlog.Frontend.Host.Extensions;

public static class ApizrConfigurationExtensions
{
    public static IServiceCollection AddServerSideApizrManagers(this IServiceCollection serviceCollection)
    {
        var serializerOptions = SystemTextJsonContentSerializer.GetDefaultJsonSerializerOptions();
        foreach (var converter in FidoBlazorSerializerContext.Default.Options.Converters)
        {
            serializerOptions.Converters.Insert(0, converter);
        }

        serviceCollection.AddScoped<HttpContextCookiesPropagationHandler>();

        serviceCollection.AddHttpClient("ReadersManagementClient")
            .ConfigureHttpClient(static c =>
            {
                c.BaseAddress = new Uri("https://localhost/api/readers");
                c.Timeout = TimeSpan.FromMinutes(1);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = false
            })
            .AddHttpMessageHandler(sp => new HttpContextCookiesPropagationHandler(
                sp.GetRequiredService<IHttpContextAccessor>(),
                excludeAuthCookies: false));
        
        serviceCollection.AddHttpClient("ReadersManagementAntiforgeryClient")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = false
            })
            .AddHttpMessageHandler(sp =>
                new HttpContextCookiesPropagationHandler(
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    excludeAuthCookies: true));

        serviceCollection.AddHttpClient("ArticlesAntiforgeryClient")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = false
            })
            .AddHttpMessageHandler(sp =>
                new HttpContextCookiesPropagationHandler(
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    excludeAuthCookies: true));

        serviceCollection.ConfigureSnowcoreBlogBackendArticlesApizrManagers(options => options
            .WithBaseAddress("https://localhost/api/articles")
            .WithRefitSettings(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
            })
            .WithRequestTimeout(TimeSpan.FromMinutes(1))
            .WithOperationTimeout(TimeSpan.FromMinutes(3))
            .ConfigureHttpClientBuilder(http => http
                .AddHttpMessageHandler(sp => new HttpContextCookiesPropagationHandler(
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    excludeAuthCookies: false))));
        
        serviceCollection.ConfigureSnowcoreBlogBackendReadersManagementApizrManagers(options => options
            .WithBaseAddress("https://localhost/api/readers")
            .WithRefitSettings(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
            })
            .WithRequestTimeout(TimeSpan.FromMinutes(1))
            .WithOperationTimeout(TimeSpan.FromMinutes(3))
            .ConfigureHttpClientBuilder(http => http
                .AddHttpMessageHandler(sp => new HttpContextCookiesPropagationHandler(
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    excludeAuthCookies: false))));

        serviceCollection.ConfigureSnowcoreBlogBackendAuthorsManagementApizrManagers(options => options
            .WithBaseAddress("https://localhost/api/authors")
            .WithRefitSettings(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
            })
            .WithRequestTimeout(TimeSpan.FromMinutes(1))
            .WithOperationTimeout(TimeSpan.FromMinutes(3))
            .ConfigureHttpClientBuilder(http => http
                .AddHttpMessageHandler(sp => new HttpContextCookiesPropagationHandler(
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    excludeAuthCookies: false))));

        return serviceCollection;
    }
}
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
        
        serviceCollection.ConfigureSnowcoreBlogBackendArticlesApizrManagers(options => options
            .WithBaseAddress("https://localhost/api/articles")
            .WithRefitSettings(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
            })
            .WithRequestTimeout(TimeSpan.FromMinutes(1))
            .WithOperationTimeout(TimeSpan.FromMinutes(3))
            .ConfigureHttpClientBuilder(http => http.AddHttpMessageHandler<HttpContextCookiesPropagationHandler>()));
        serviceCollection.ConfigureSnowcoreBlogBackendReadersManagementApizrManagers(options => options
            .WithBaseAddress("https://localhost/api/readers")
            .WithRefitSettings(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
            })
            .WithRequestTimeout(TimeSpan.FromMinutes(1))
            .WithOperationTimeout(TimeSpan.FromMinutes(3))
            .ConfigureHttpClientBuilder(http => http.AddHttpMessageHandler<HttpContextCookiesPropagationHandler>()));
            
        return serviceCollection;
    }
}
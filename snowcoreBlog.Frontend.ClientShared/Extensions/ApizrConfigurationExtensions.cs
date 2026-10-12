using Microsoft.Extensions.DependencyInjection;
using Refit;
using snowcoreBlog.Frontend.ClientShared.Handlers;
using snowcoreBlog.Frontend.Infrastructure.Context;
using snowcoreBlog.Frontend.Infrastructure.Extensions;
using snowcoreBlog.PublicApi.Api;

namespace snowcoreBlog.Frontend.ClientShared.Extensions;

public static class ApizrConfigurationExtensions
{
    public static IServiceCollection AddClientSideApizrManagers(this IServiceCollection serviceCollection)
    {
        var serializerOptions = SystemTextJsonContentSerializer.GetDefaultJsonSerializerOptions();
        foreach (var converter in FidoBlazorSerializerContext.Default.Options.Converters)
        {
            serializerOptions.Converters.Insert(0, converter);
        }
        
        serviceCollection.AddScoped<IncludeCookiesHandler>();

        // Named client for the server-sent events flows of the Readers Management
        // service (SSE streaming responses are not expressible through the
        // Refit-based Apizr managers). NOTE: the base address must keep its
        // trailing slash so relative endpoint paths append to "/api/readers"
        // instead of replacing its last segment during URI combination.
        serviceCollection.AddHttpClient("ReadersManagementSseClient", static client =>
        {
            client.BaseAddress = new Uri("https://localhost/api/readers/");
            client.Timeout = TimeSpan.FromMinutes(3);
        })
        .AddAntiforgeryRetryHandler()
        .AddHttpMessageHandler<IncludeCookiesHandler>();

        serviceCollection.ConfigureSnowcoreBlogBackendArticlesApizrManagers(options => options
            .WithBaseAddress("https://localhost/api/articles")
            .WithRefitSettings(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
            })
            .WithRequestTimeout(TimeSpan.FromMinutes(1))
            .WithOperationTimeout(TimeSpan.FromMinutes(3))
            .WithHttpMessageHandler<IncludeCookiesHandler>());
        serviceCollection.ConfigureSnowcoreBlogBackendReadersManagementApizrManagers(options => options
            .WithBaseAddress("https://localhost/api/readers")
            .WithRefitSettings(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
            })
            .WithRequestTimeout(TimeSpan.FromMinutes(1))
            .WithOperationTimeout(TimeSpan.FromMinutes(3))
            .WithHttpMessageHandler<IncludeCookiesHandler>());

        serviceCollection.ConfigureSnowcoreBlogBackendAuthorsManagementApizrManagers(options => options
            .WithBaseAddress("https://localhost/api/authors")
            .WithRefitSettings(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
            })
            .WithRequestTimeout(TimeSpan.FromMinutes(1))
            .WithOperationTimeout(TimeSpan.FromMinutes(3))
            .WithHttpMessageHandler<IncludeCookiesHandler>());
            
        return serviceCollection;
    }
}
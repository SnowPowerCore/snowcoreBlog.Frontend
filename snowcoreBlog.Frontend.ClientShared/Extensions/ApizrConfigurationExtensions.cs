using Microsoft.Extensions.DependencyInjection;
using Refit;
using snowcoreBlog.Frontend.ClientShared.Handlers;
using snowcoreBlog.Frontend.Infrastructure.Context;
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
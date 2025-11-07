using Blazored.LocalStorage;
using Blazored.SessionStorage;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Refit;
using snowcoreBlog.ApplicationLaunch.Implementations.BackgroundServices;
using snowcoreBlog.ApplicationLaunch.Interfaces;
using snowcoreBlog.Frontend.Articles.Extensions;
using snowcoreBlog.Frontend.ClientShared.Handlers;
using snowcoreBlog.Frontend.ClientShared.Services;
using snowcoreBlog.Frontend.Infrastructure.Context;
using snowcoreBlog.Frontend.Infrastructure.Extensions;
using snowcoreBlog.Frontend.ReadersManagement.Extensions;
using snowcoreBlog.Frontend.SharedComponents.Extensions;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Validation.Dto;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClient(this IServiceCollection serviceCollection)
    {
        var serializerOptions = SystemTextJsonContentSerializer.GetDefaultJsonSerializerOptions();
        foreach (var converter in FidoBlazorSerializerContext.Default.Options.Converters)
        {
            serializerOptions.Converters.Insert(0, converter);
        }
        
        serviceCollection.AddArticles();
        serviceCollection.AddReadersManagement();
        serviceCollection.AddSharedComponents();
        serviceCollection.AddBlazoredLocalStorage();
        serviceCollection.AddBlazoredSessionStorage();
        serviceCollection.AddTimeWarpState(static options =>
        {
            options.Assemblies = [
                typeof(Articles.Extensions.ServiceCollectionExtensions).Assembly,
                typeof(ReadersManagement.Extensions.ServiceCollectionExtensions).Assembly,
                typeof(SharedComponents.Extensions.ServiceCollectionExtensions).Assembly,
                typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly
            ];
        });
        serviceCollection.AddWebAuthn();
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
        serviceCollection.AddFluentUIComponents();

        serviceCollection.AddAuthorizationCore();
        serviceCollection.AddCascadingAuthenticationState();
        serviceCollection.AddScoped<IncludeCookiesHandler>();
        
        serviceCollection.AddSingleton<IValidator<RequestCreateReaderAccountDto>, RequestCreateReaderAccountValidator>();
        serviceCollection.AddSingleton<IValidator<RequestAssertionOptionsDto>, RequestAssertionOptionsValidator>();
        serviceCollection.AddSingleton<IValidator<RequestAttestationOptionsDto>, RequestAttestationOptionsValidator>();
        serviceCollection.AddSingleton<IValidator<ConfirmCreateReaderAccountDto>, ConfirmCreateReaderAccountValidator>();

        serviceCollection.AddSingleton<IApplicationLaunchService>(static sp =>
            new ClientSharedApplicationLaunchService(sp));
        serviceCollection.AddHostedService(static sp =>
            new ApplicationLaunchWorker(sp.GetRequiredService<IHostApplicationLifetime>(),
                sp.GetRequiredService<IApplicationLaunchService>()));
        
        return serviceCollection;
    }
}
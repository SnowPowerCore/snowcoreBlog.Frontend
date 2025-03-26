using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Morris.Blazor.Validation;
using snowcoreBlog.Frontend.Infrastructure.Extensions;
using snowcoreBlog.Frontend.Infrastructure.Providers;
using snowcoreBlog.Frontend.ReadersManagement.Extensions;
using snowcoreBlog.Frontend.SharedComponents.Extensions;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.Utilities.Api;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddReadersManagement();
        serviceCollection.AddSharedComponents();
        serviceCollection.AddBlazoredLocalStorage();
        serviceCollection.AddBlazoredSessionStorage();
        serviceCollection.AddTimeWarpState(static options =>
        {
            options.Assemblies = [
                typeof(ReadersManagement.Extensions.ServiceCollectionExtensions).Assembly,
                typeof(SharedComponents.Extensions.ServiceCollectionExtensions).Assembly,
                typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly
            ];
        });
        serviceCollection.AddWebAuthn();
        serviceCollection.ConfigureSnowcoreBlogBackendReadersManagementApizrManagers(options =>
            options.WithBaseAddress("https://localhost/api/readers"));
        serviceCollection.AddFluentUIComponents();
        serviceCollection.AddFormValidation(static config =>
            config.AddFluentValidation(typeof(ApiResponse).Assembly));

        serviceCollection.AddAuthorizationCore();
        serviceCollection.AddCascadingAuthenticationState();
        serviceCollection.AddScoped<AuthenticationStateProvider, BlogAuthStateProvider>();
        return serviceCollection;
    }
}
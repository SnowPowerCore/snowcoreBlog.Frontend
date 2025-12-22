using Blazored.LocalStorage;
using Blazored.SessionStorage;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using snowcoreBlog.ApplicationLaunch.Interfaces;
using snowcoreBlog.Frontend.AuthorsManagement.Extensions;
using snowcoreBlog.Frontend.Articles.Extensions;
using snowcoreBlog.Frontend.ClientShared.Services;
using snowcoreBlog.Frontend.Infrastructure.Extensions;
using snowcoreBlog.Frontend.ServiceNotifications.Extensions;
using snowcoreBlog.Frontend.ReadersManagement.Extensions;
using snowcoreBlog.Frontend.SharedComponents.Extensions;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Validation.Dto;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddArticles();
        serviceCollection.AddServiceNotifications();
        serviceCollection.AddAuthorsManagement();
        serviceCollection.AddReadersManagement();
        serviceCollection.AddSharedComponents();
        serviceCollection.AddBlazoredLocalStorage();
        serviceCollection.AddBlazoredSessionStorage();
        serviceCollection.AddTimeWarpState(static options =>
        {
            options.Assemblies = [
                typeof(_Imports).Assembly,
                typeof(SharedComponents._Imports).Assembly,
                typeof(Articles._Imports).Assembly,
                typeof(AuthorsManagement._Imports).Assembly,
                typeof(ServiceNotifications._Imports).Assembly,
                typeof(ReadersManagement._Imports).Assembly,
                typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly
            ];
        });
        serviceCollection.AddWebAuthn();
        serviceCollection.AddFluentUIComponents();

        serviceCollection.AddAuthorizationCore();
        serviceCollection.AddCascadingAuthenticationState();

        serviceCollection.AddSingleton<IValidator<RequestCreateReaderAccountDto>, RequestCreateReaderAccountValidator>();
        serviceCollection.AddSingleton<IValidator<RequestAssertionOptionsDto>, RequestAssertionOptionsValidator>();
        serviceCollection.AddSingleton<IValidator<RequestAttestationOptionsDto>, RequestAttestationOptionsValidator>();
        serviceCollection.AddSingleton<IValidator<ConfirmCreateReaderAccountDto>, ConfirmCreateReaderAccountValidator>();

        serviceCollection.AddScoped<IApplicationLaunchService>(static sp =>
            new FrontendApplicationLaunchService(sp.GetRequiredService<IStore>()));

        return serviceCollection;
    }
}
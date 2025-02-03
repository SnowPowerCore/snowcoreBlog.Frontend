using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Morris.Blazor.Validation;
using snowcoreBlog.Frontend.Infrastructure.Providers;
using snowcoreBlog.Frontend.ReadersManagement.Extensions;
using snowcoreBlog.Frontend.SharedComponents.Extensions;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.Utilities.Api;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.Client;

public class Program
{
    private static Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        });
        ConfigureServices(builder.Services);

        return builder.Build().RunAsync();
    }

    public static void ConfigureServices(IServiceCollection serviceCollection)
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
        serviceCollection.ConfigureSnowcoreBlogBackendReadersManagementApizrManagers(options =>
            options.WithBaseAddress("https://localhost:5050"));
        serviceCollection.AddFluentUIComponents();
        serviceCollection.AddFormValidation(static config =>
            config.AddFluentValidation(typeof(ApiResponse).Assembly));

        serviceCollection.AddAuthorizationCore();
        serviceCollection.AddScoped<AuthenticationStateProvider, BlogAuthStateProvider>();
    }
}
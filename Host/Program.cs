using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Morris.Blazor.Validation;
using snowcoreBlog.Frontend.Host;
using snowcoreBlog.Frontend.Infrastructure.Providers;
using snowcoreBlog.Frontend.ReadersManagement.Extensions;
using snowcoreBlog.Frontend.SharedComponents.Extensions;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.Utilities.Api;
using TimeWarp.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTimeWarpState(options =>
{
    options.Assemblies = [
        typeof(snowcoreBlog.Frontend.ReadersManagement.Extensions.WebApplicationBuilderExtensions).Assembly,
        typeof(snowcoreBlog.Frontend.SharedComponents.Extensions.WebApplicationBuilderExtensions).Assembly,
        typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly
    ];
});
builder.AddReadersManagement();
builder.AddSharedComponents();
builder.Services.ConfigureSnowcoreBlogBackendReadersManagementApizrManagers(default);
builder.Services.AddFluentUIComponents();
builder.Services.AddFormValidation(static config =>
    config.AddFluentValidation(typeof(ApiResponse).Assembly));
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, BlogAuthStateProvider>();

await builder.Build().RunAsync();
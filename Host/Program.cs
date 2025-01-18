using Apizr;
using Apizr.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Morris.Blazor.Validation;
using snowcoreBlog.Frontend.Host;
using snowcoreBlog.Frontend.Infrastructure.Providers;
using snowcoreBlog.Frontend.ReadersManagement.Extensions;
using snowcoreBlog.PublicApi.Utilities.Api;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.AddReadersManagement();
builder.Services.AddApizr(
    registry => { },
    config => config
        .ConfigureHttpClientBuilder(builder => builder.AddStandardResilienceHandler())
        .WithLogging(
            HttpTracerMode.ExceptionsOnly,
            HttpMessageParts.ResponseAll,
            LogLevel.Error)
);
builder.Services.AddFluentUIComponents();
builder.Services.AddFormValidation(config =>
    config.AddFluentValidation(typeof(ApiResponse).Assembly));
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, BlogAuthStateProvider>();

await builder.Build().RunAsync();
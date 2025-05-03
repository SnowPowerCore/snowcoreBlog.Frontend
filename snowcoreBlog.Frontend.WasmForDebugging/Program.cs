using BitzArt.Blazor.Auth;
using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using snowcoreBlog.Frontend.ClientShared.Extensions;
using snowcoreBlog.Frontend.ClientShared.Handlers;
using snowcoreBlog.Frontend.WasmForDebugging;
using snowcoreBlog.Frontend.WasmForDebugging.Providers;
using snowcoreBlog.Frontend.WasmForDebugging.Services;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
{
    ValidateScopes = true,
    ValidateOnBuild = true
}));
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services
    .AddHttpClient(string.Empty, sp => sp.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<IncludeCookiesHandler>();
builder.Services.AddSingleton(static sp => Altcha.CreateSolverBuilder().Build());
builder.Services.AddClient();

builder.Services.AddScoped<AuthenticationStateProvider, WasmCookiesAuthenticationStateProvider>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserService<LoginByAssertionDto>, UserService<LoginByAssertionDto>>();

await builder.Build().RunAsync();
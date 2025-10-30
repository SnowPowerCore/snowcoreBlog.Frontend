using System.Net;
using BitzArt.Blazor.Auth.Server;
using Ixnas.AltchaNet;
using snowcoreBlog.Frontend.ClientShared.Extensions;
using snowcoreBlog.Frontend.Host.Components;
using snowcoreBlog.Frontend.Host.Services;
using snowcoreBlog.PublicApi.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(static (c, opts) =>
{
    opts.ValidateScopes = true;
    opts.ValidateOnBuild = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents()
	.AddInteractiveWebAssemblyComponents();

var container = new CookieContainer();
builder.Services.AddSingleton(container);

builder.Services.AddSingleton(static sp => Altcha.CreateSolverBuilder().Build());
builder.Services.AddClient();
builder.Services.ConfigureSnowcoreBlogBackendReadersManagementApizrManagers(options =>
	options.WithHttpClientHandler(sp => new()
	{
		CookieContainer = container,
        UseCookies = true
	}));
builder.AddBlazorAuth<GlobalReaderAccountAuthenticationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode()
	.AddInteractiveWebAssemblyRenderMode()
	.AddAdditionalAssemblies(
		typeof(snowcoreBlog.Frontend.Client.Program).Assembly,
		typeof(snowcoreBlog.Frontend.Articles.Extensions.ServiceCollectionExtensions).Assembly,
		typeof(snowcoreBlog.Frontend.ReadersManagement.Extensions.ServiceCollectionExtensions).Assembly,
		typeof(snowcoreBlog.Frontend.SharedComponents.Extensions.ServiceCollectionExtensions).Assembly);

app.MapAuthEndpoints();

await app.RunAsync();
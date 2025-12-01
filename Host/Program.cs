using BitzArt.Blazor.Auth.Server;
using Ixnas.AltchaNet;
using snowcoreBlog.Frontend.ClientShared.Extensions;
using snowcoreBlog.Frontend.Host.Components;
using snowcoreBlog.Frontend.Host.Extensions;
using snowcoreBlog.Frontend.Host.Middleware;
using snowcoreBlog.Frontend.Host.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(static (c, opts) =>
{
    opts.ValidateScopes = true;
    opts.ValidateOnBuild = true;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(static sp => Altcha.CreateSolverBuilder().Build());
builder.Services.AddClient();
builder.Services.AddServerSideApizrManagers();
builder.AddBlazorAuth<GlobalReaderAccountAuthenticationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseMiddleware<ApplyBasePathMiddleware>();
app.UseMiddleware<ArticlesAntiforgeryInitMiddleware>();
app.UseMiddleware<ReadersManagementAntiforgeryInitMiddleware>();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
	.AddAdditionalAssemblies(
		typeof(snowcoreBlog.Frontend.Client._Imports).Assembly,
		typeof(snowcoreBlog.Frontend.ClientShared._Imports).Assembly,
		typeof(snowcoreBlog.Frontend.Articles._Imports).Assembly,
		typeof(snowcoreBlog.Frontend.ReadersManagement._Imports).Assembly,
		typeof(snowcoreBlog.Frontend.SharedComponents._Imports).Assembly);
app.MapAuthEndpoints();

await app.RunAsync();
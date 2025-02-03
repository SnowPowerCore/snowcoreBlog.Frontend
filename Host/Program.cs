using snowcoreBlog.Frontend.Host.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents()
	.AddInteractiveWebAssemblyComponents();

snowcoreBlog.Frontend.Client.Program.ConfigureServices(builder.Services);

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
		typeof(snowcoreBlog.Frontend.ReadersManagement.Extensions.ServiceCollectionExtensions).Assembly,
		typeof(snowcoreBlog.Frontend.SharedComponents.Extensions.ServiceCollectionExtensions).Assembly);

await app.RunAsync();
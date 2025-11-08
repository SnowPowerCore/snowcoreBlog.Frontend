using Microsoft.Extensions.DependencyInjection;
using snowcoreBlog.ApplicationLaunch.Interfaces;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Services;

public class ClientSharedApplicationLaunchService : IApplicationLaunchService
{
    private readonly IServiceProvider _serviceProvider;

    public ClientSharedApplicationLaunchService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task InitAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        using var articlesAntiforgery = scope.ServiceProvider.GetRequiredService<IStore>().GetState<Articles.Features.Antiforgery.ArticlesAntiforgeryState>();
        using var readersManagementAntiforgery = scope.ServiceProvider.GetRequiredService<IStore>().GetState<ReadersManagement.Features.Antiforgery.ReaderAccountAntiforgeryState>();
        await articlesAntiforgery.GetAndSetRequestVerificationToken(CancellationToken.None);
        await readersManagementAntiforgery.GetAndSetRequestVerificationToken(CancellationToken.None);
    }
}
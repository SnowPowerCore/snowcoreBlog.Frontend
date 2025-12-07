using snowcoreBlog.ApplicationLaunch.Interfaces;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Services;

public class FrontendApplicationLaunchService(IStore store) : IApplicationLaunchService
{
    public async Task InitAsync()
    {
        using var articlesAntiforgeryState = store.GetState<Articles.Features.Antiforgery.AntiforgeryState>();
        using var readerAccountAntiforgeryState = store.GetState<ReadersManagement.Features.Antiforgery.AntiforgeryState>();
        
        await articlesAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None);
        await readerAccountAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None);
    }
}
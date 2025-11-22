using snowcoreBlog.ApplicationLaunch.Interfaces;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Services;

public class FrontendApplicationLaunchService : IApplicationLaunchService
{
    private readonly IStore _store;

    public FrontendApplicationLaunchService(IStore store)
    {
        _store = store;
    }

    public async Task InitAsync()
    {
        using var articlesAntiforgeryState = _store.GetState<Articles.Features.Antiforgery.AntiforgeryState>();
        using var readerAccountAntiforgeryState = _store.GetState<ReadersManagement.Features.Antiforgery.AntiforgeryState>();
        
        await articlesAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None);
        await readerAccountAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None);
    }
}
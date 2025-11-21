using snowcoreBlog.ApplicationLaunch.Interfaces;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Services;

public class ApplicationLaunchService : IApplicationLaunchService
{
    private readonly IStore _store;

    public ApplicationLaunchService(IStore store)
    {
        _store = store;
    }

    public async Task InitAsync()
    {
        using var articlesAntiforgeryState = _store.GetState<Articles.Features.Antiforgery.AntiforgeryState>();
        using var readerAccountAntiforgeryState = _store.GetState<ReadersManagement.Features.Antiforgery.AntiforgeryState>();
        
        var articlesAntiforgeryTask = articlesAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None);
        var readerAccountAntiforgeryTask = readerAccountAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None);
        
        await Task.WhenAll(articlesAntiforgeryTask, readerAccountAntiforgeryTask);
    }
}
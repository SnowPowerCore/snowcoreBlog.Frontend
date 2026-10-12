using snowcoreBlog.ApplicationLaunch.Interfaces;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Services;

public class FrontendApplicationLaunchService(IStore store) : IApplicationLaunchService
{
    // Both boot roots trigger the launch (Host AppInit.razor and the WASM
    // App.razor), and inside a WebAssembly app both resolve this scoped service
    // from the same root scope. Coalesce the calls into a single run: the
    // antiforgery states are cached by the store, so previously two concurrent
    // launches raced each other — the first run disposing the states canceled
    // the shared state tokens and aborted the other run's in-flight antiforgery
    // token requests, leaving RequestVerificationToken empty for the very first
    // submit.
    private Task? _launchTask;

    public Task InitAsync() => _launchTask ??= LaunchCoreAsync();

    private async Task LaunchCoreAsync()
    {
        try
        {
            var articlesAntiforgeryState = store.GetState<Articles.Features.Antiforgery.AntiforgeryState>();
            var readerAccountAntiforgeryState = store.GetState<ReadersManagement.Features.Antiforgery.AntiforgeryState>();
            var authorsAntiforgeryState = store.GetState<AuthorsManagement.Features.Antiforgery.AntiforgeryState>();

            await Task.WhenAll(
                articlesAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None),
                readerAccountAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None),
                authorsAntiforgeryState.GetAndSetRequestVerificationToken(CancellationToken.None));
        }
        finally
        {
            _launchTask = default;
        }
    }
}
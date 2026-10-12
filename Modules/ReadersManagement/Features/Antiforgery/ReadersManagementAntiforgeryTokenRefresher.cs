using snowcoreBlog.Frontend.Core.Interfaces.Antiforgery;
using snowcoreBlog.Frontend.Infrastructure.Antiforgery;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ReadersManagement.Features.Antiforgery;

/// <summary>
/// Bridges the module-agnostic <see cref="AntiforgeryRetryHandler"/> to this
/// module's <see cref="AntiforgeryState"/>, so HTTP pipelines can recover from
/// missing or stale tokens without any page-level coordination.
/// </summary>
internal sealed class ReadersManagementAntiforgeryTokenRefresher(IStore store) : IAntiforgeryTokenRefresher
{
    public async Task<string?> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken = default)
    {
        var state = store.GetState<AntiforgeryState>();
        if (forceRefresh || string.IsNullOrEmpty(state.RequestVerificationToken))
        {
            await state.GetAndSetRequestVerificationToken(cancellationToken);
            state = store.GetState<AntiforgeryState>();
        }

        return state.RequestVerificationToken;
    }
}
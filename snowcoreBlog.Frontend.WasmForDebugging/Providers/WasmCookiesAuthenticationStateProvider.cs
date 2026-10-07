using System.Security.Claims;
using BitzArt.Blazor.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace snowcoreBlog.Frontend.WasmForDebugging.Providers;

/// <summary>
/// The backend refresh endpoint requires a still-valid access token, so an expired
/// pair cannot be recovered after the fact. This provider therefore rotates the JWT
/// pair proactively: it schedules a refresh shortly before the access token's
/// "exp" claim (configurable margin), rotates the pair, re-resolves the
/// authentication state and re-publishes it to all subscribers.
/// </summary>
public sealed class WasmCookiesAuthenticationStateProvider(IUserService user, IConfiguration configuration)
    : AuthenticationStateProvider, IDisposable
{
    private const string ExpiryClaimType = "exp";

    // Rotate within the last 5 minutes of the access token's lifetime by default.
    private static readonly TimeSpan DefaultRefreshMargin = TimeSpan.FromMinutes(5);

    // After a failed rotation, block re-attempts for this long. Without it, every
    // auth-state resolution inside the rotation window retries the refresh call
    // immediately, which once produced a ~10 req/s request storm.
    private static readonly TimeSpan RotationFailureCooldown = TimeSpan.FromSeconds(30);

    private Timer? _proactiveRefreshTimer;
    private int _refreshInFlight;
    private DateTimeOffset _rotationBlockedUntil;
    private bool _disposed;

    private TimeSpan RefreshMargin
    {
        get
        {
            var minutes = configuration.GetValue<double?>("Auth:ProactiveRefreshMarginMinutes");
            return minutes is > 0 ? TimeSpan.FromMinutes(minutes.Value) : DefaultRefreshMargin;
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = await ResolveWithProactiveRotationAsync();

        NotifyAuthenticationStateChanged(Task.FromResult(state));
        ScheduleProactiveRefresh(state.User);

        return state;
    }

    private async Task<AuthenticationState> ResolveWithProactiveRotationAsync()
    {
        var state = await user.GetAuthenticationStateAsync();

        if (DateTimeOffset.UtcNow >= _rotationBlockedUntil &&
            IsAuthenticated(state.User) &&
            GetRemainingLifetime(state.User) is { } remaining &&
            remaining < RefreshMargin)
        {
            // Already inside the rotation window (e.g. a long-idle tab): rotate now,
            // then resolve the state again with the fresh pair.
            var refreshed = await TryRotateAsync();
            if (refreshed is not null)
                state = refreshed;
        }

        return state;
    }

    private void ScheduleProactiveRefresh(ClaimsPrincipal principal)
    {
        _proactiveRefreshTimer?.Dispose();
        _proactiveRefreshTimer = null;

        if (_disposed || !IsAuthenticated(principal) || GetRemainingLifetime(principal) is not { } remaining)
            return;

        var due = remaining - RefreshMargin;
        if (due < TimeSpan.Zero)
            due = TimeSpan.Zero;

        // Clamp to a safe timer range; longer idle periods are handled by the
        // in-window check on the next authentication state resolution.
        if (due > TimeSpan.FromDays(20))
            due = TimeSpan.FromDays(20);

        Console.WriteLine($"[AuthRefresh] Proactive rotation scheduled in {due} (remaining token lifetime: {remaining}).");
        _proactiveRefreshTimer = new Timer(OnProactiveRefreshTick, null, due, Timeout.InfiniteTimeSpan);
    }

    private async void OnProactiveRefreshTick(object? state)
    {
        if (Interlocked.Exchange(ref _refreshInFlight, 1) == 1)
            return;

        try
        {
            // Re-resolve first: another tab may have rotated the pair already, in which
            // case the shared cookies hold a fresh token that must not be rotated again
            // unless it is actually inside the rotation window.
            var current = await user.GetAuthenticationStateAsync();

            if (DateTimeOffset.UtcNow >= _rotationBlockedUntil &&
                IsAuthenticated(current.User) &&
                GetRemainingLifetime(current.User) is { } remaining &&
                remaining < RefreshMargin)
            {
                Console.WriteLine("[AuthRefresh] Rotation window entered, rotating JWT pair...");
                var refreshed = await TryRotateAsync();
                if (refreshed is not null)
                {
                    current = refreshed;
                    Console.WriteLine("[AuthRefresh] JWT pair rotated successfully.");
                }
                else
                {
                    Console.WriteLine("[AuthRefresh] JWT pair rotation failed.");
                }
            }

            NotifyAuthenticationStateChanged(Task.FromResult(current));
            ScheduleProactiveRefresh(current.User);
        }
        catch
        {
            // Never let a timer callback crash the host; the next state resolution
            // will recover or show the signed-out state.
        }
        finally
        {
            Interlocked.Exchange(ref _refreshInFlight, 0);
        }
    }

    private async Task<AuthenticationState?> TryRotateAsync()
    {
        try
        {
            // The refresh-token cookie (".DotNet.Application.User.SystemUpdateKey") is
            // attached automatically by IncludeCookiesHandler; the backend resolves it
            // from the cookie when the body token is empty and rotates BOTH cookies
            // (the still-valid access token is required by the refresh endpoint).
            var info = await user.RefreshJwtPairAsync(string.Empty);
            if (!info.IsSuccess)
            {
                // Back off: a persistently failing refresh (backend down, contract
                // mismatch, …) must degrade to "stay signed out at expiry", not to
                // a tight retry loop driven by every auth-state query.
                _rotationBlockedUntil = DateTimeOffset.UtcNow + RotationFailureCooldown;
                return null;
            }

            return await user.GetAuthenticationStateAsync();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsAuthenticated(ClaimsPrincipal principal) =>
        principal.Identity?.IsAuthenticated == true;

    private static TimeSpan? GetRemainingLifetime(ClaimsPrincipal principal)
    {
        var expValue = principal.FindFirst(ExpiryClaimType)?.Value;
        if (!long.TryParse(expValue, out var expSeconds))
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(expSeconds) - DateTimeOffset.UtcNow;
    }

    public void Dispose()
    {
        _disposed = true;
        _proactiveRefreshTimer?.Dispose();
        _proactiveRefreshTimer = null;
    }
}
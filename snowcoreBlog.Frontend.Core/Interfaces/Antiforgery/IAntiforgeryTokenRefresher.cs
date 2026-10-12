namespace snowcoreBlog.Frontend.Core.Interfaces.Antiforgery;

/// <summary>
/// Source of the antiforgery request token for a backend service, implemented
/// by every frontend module that owns an <c>AntiforgeryState</c>. The
/// <see cref="AntiforgeryRetryHandler"/> depends on this abstraction so the
/// retry pipeline stays module-agnostic.
/// </summary>
public interface IAntiforgeryTokenRefresher
{
    /// <summary>
    /// Returns the current request verification token, fetching a fresh one
    /// when <paramref name="forceRefresh"/> is set or when the cached token is
    /// empty. Returns <see langword="null"/> when no token can be obtained.
    /// </summary>
    Task<string?> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken = default);
}
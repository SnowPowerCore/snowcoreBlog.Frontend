using System.Security.Claims;
using Apizr;
using BitzArt.Blazor.Auth;
using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Components.Authorization;
using snowcoreBlog.Frontend.ReadersManagement.Features.Antiforgery;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Extensions;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.WasmForDebugging.Services;

public class UserService(IApizrManager<IReaderAccountManagementApi> readerAccountApi,
                         IApizrManager<IReaderAccountTokensApi> tokensApi) : IUserService
{
    // The identity authentication type MUST be non-empty: ClaimsIdentity created without
    // one reports IsAuthenticated == false, so AuthorizeView renders NotAuthorized
    // even when valid claims exist.
    private const string AuthenticationType = "Cookies";

    private protected IApizrManager<IReaderAccountManagementApi> ReaderAccountApi { get; } = readerAccountApi;
    private protected IApizrManager<IReaderAccountTokensApi> TokensApi { get; } = tokensApi;

    public async Task<AuthenticationState> GetAuthenticationStateAsync(CancellationToken cancellationToken = default)
    {
        // NOTE: no refresh attempt on failure here — the backend refresh endpoint
        // requires a still-valid access token, so an expired pair cannot be recovered.
        // Proactive rotation before expiry is handled by WasmCookiesAuthenticationStateProvider.
        return await TryResolveAuthenticationStateAsync(cancellationToken) ?? AnonymousAuthenticationState();
    }

    private async Task<AuthenticationState?> TryResolveAuthenticationStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await ReaderAccountApi.ExecuteAsync(static (opt, api) =>
                api.RequestAuthData(opt), o => o.WithCancellation(cancellationToken));
            if (!response.IsSuccess)
                return null;

            var data = response.ToData<AuthenticationStateDto>(out _);
            if (data is default(AuthenticationStateDto) || data.Claims.Count == 0)
                return null;

            var identity = new ClaimsIdentity(
                data.Claims.Select(x => new Claim(x.Key, x.Value)),
                authenticationType: AuthenticationType);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // Network failure, CORS problem, Apizr exception, ... — treat as anonymous
            // instead of faulting the authentication state task.
            return null;
        }
    }

    private static AuthenticationState AnonymousAuthenticationState() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        // The auth cookies are HTTP-only and cannot be cleared from WebAssembly,
        // so the backend revoke endpoint (tokens/revoke/v1) does it for us: it
        // deletes the refresh-token record in Redis and expires both cookies on
        // the response, which IncludeCookiesHandler lets the browser store. The
        // caller must force-reload afterwards to re-resolve the anonymous state.
        using var antiforgeryResponse = await TokensApi.ExecuteAsync(
            static (opt, api) => api.GetAntiforgeryToken(opt), o => o.WithCancellation(cancellationToken));
        var antiforgeryData = antiforgeryResponse.ToData<AntiforgeryResultDto>(out _);

        var token = antiforgeryData?.RequestToken ?? string.Empty;
        using var revokeResponse = await TokensApi.ExecuteAsync((opt, api) => api.RevokeReaderTokens(token, opt),
            o => o.WithCancellation(cancellationToken));

        if (!revokeResponse.IsSuccess)
            throw new InvalidOperationException(
                revokeResponse.Exception?.Message ?? "Sign-out failed: the backend could not revoke the token pair.");
    }

    public async Task<AuthenticationOperationInfo> RefreshJwtPairAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // The refresh-token cookie (".DotNet.Application.User.SystemUpdateKey") is sent
        // automatically by IncludeCookiesHandler (BrowserRequestCredentials.Include),
        // so the backend ResolveRefreshTokenStep resolves it from the cookie even when
        // the WASM caller cannot read it. The rotated access/refresh cookies returned
        // via Set-Cookie are stored by the browser automatically.
        using var response = await TokensApi.ExecuteAsync(
            (opt, api) => api.RefreshReaderJwtPair(refreshToken, opt),
            o => o.WithCancellation(cancellationToken));

        if (!response.IsSuccess)
            return new AuthenticationOperationInfo(errorMessage: response.Exception?.Message ?? "Token refresh failed");

        return new AuthenticationOperationInfo(isSuccess: true);
    }
}

public class UserService<TSignInPayload>(IApizrManager<IReaderAccountManagementApi> readerAccountApi,
                                         IApizrManager<IReaderAccountTokensApi> tokensApi,
                                         AltchaSolver altchaSolver,
                                         IStore store)
    : UserService(readerAccountApi, tokensApi), IUserService<LoginByAssertionDto>
{
    public async Task<AuthenticationOperationInfo> SignInAsync(LoginByAssertionDto signInPayload, CancellationToken cancellationToken = default)
    {
        // One-time code sign-in completes through the OTP verification endpoint;
        // passkey sign-in completes through the assertion endpoint.
        if (!string.IsNullOrWhiteSpace(signInPayload.OneTimeCode))
            return await SignInByOneTimeCodeAsync(signInPayload, cancellationToken);

        using var antiforgeryState = store.GetState<AntiforgeryState>();
        if (antiforgeryState is default(AntiforgeryState))
            return new AuthenticationOperationInfo(errorMessage: "No antiforgery token");

        using var captchaResponse = await TokensApi.ExecuteAsync(static (opt, api) => api.GetAltchaChallenge(opt), o => o.WithCancellation(cancellationToken));
        if (!captchaResponse.IsSuccess)
            return new AuthenticationOperationInfo(errorMessage: captchaResponse.Exception?.Message);

        var solverResult = altchaSolver.Solve(captchaResponse.Result);
        if (!solverResult.Success)
            return new AuthenticationOperationInfo(errorMessage: solverResult.Error.Message);

        using var response = await ReaderAccountApi.ExecuteAsync((opt, api) =>
            api.LoginByAssertion(signInPayload, antiforgeryState.RequestVerificationToken, solverResult.Altcha, opt), o => o.WithCancellation(cancellationToken));
        var data = response.ToData<LoginByAssertionResultDto>(out var errors);
        if (data is default(LoginByAssertionResultDto) || errors.Count > 0)
            return new AuthenticationOperationInfo(errorMessage: errors.FirstOrDefault());
        return new AuthenticationOperationInfo(true);
    }

    private async Task<AuthenticationOperationInfo> SignInByOneTimeCodeAsync(LoginByAssertionDto signInPayload, CancellationToken cancellationToken = default)
    {
        using var antiforgeryState = store.GetState<AntiforgeryState>();
        if (antiforgeryState is default(AntiforgeryState) || string.IsNullOrEmpty(antiforgeryState.RequestVerificationToken))
            return new AuthenticationOperationInfo(errorMessage: "No antiforgery token");

        using var captchaResponse = await TokensApi.ExecuteAsync(static (opt, api) => api.GetAltchaChallenge(opt), o => o.WithCancellation(cancellationToken));
        if (!captchaResponse.IsSuccess)
            return new AuthenticationOperationInfo(errorMessage: captchaResponse.Exception?.Message);

        var solverResult = altchaSolver.Solve(captchaResponse.Result);
        if (!solverResult.Success)
            return new AuthenticationOperationInfo(errorMessage: solverResult.Error.Message);

        // The rotated auth cookies returned via Set-Cookie are stored by the
        // browser automatically (IncludeCookiesHandler sends the request with
        // credentials), so the subsequent auth-state resolution sees the login.
        using var response = await ReaderAccountApi.ExecuteAsync((opt, api) =>
            api.VerifyOneTimeCode(new VerifyOneTimeCodeDto { Email = signInPayload.Email, Code = signInPayload.OneTimeCode! }, antiforgeryState.RequestVerificationToken, solverResult.Altcha, opt), o => o.WithCancellation(cancellationToken));
        var data = response.ToData<LoginByOneTimeCodeResultDto>(out var errors);
        if (data is default(LoginByOneTimeCodeResultDto) || errors.Count > 0)
            return new AuthenticationOperationInfo(errorMessage: errors.FirstOrDefault());
        return new AuthenticationOperationInfo(true);
    }
}
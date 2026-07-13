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
    private protected IApizrManager<IReaderAccountManagementApi> ReaderAccountApi { get; } = readerAccountApi;
    private protected IApizrManager<IReaderAccountTokensApi> TokensApi { get; } = tokensApi;

    public async Task<AuthenticationState> GetAuthenticationStateAsync(CancellationToken cancellationToken = default)
    {
        using var response = await ReaderAccountApi.ExecuteAsync(static (opt, api) =>
            api.RequestAuthData(opt), o => o.WithCancellation(cancellationToken));
        var data = response.ToData<AuthenticationStateDto>(out var errors);
        if (data is default(AuthenticationStateDto) && errors.Count > 0)
            return new AuthenticationState(new ClaimsPrincipal());
        return new AuthenticationState(new(new ClaimsIdentity(data!.Claims.Select(x => new Claim(x.Key, x.Value)))));
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        // No backend revoke/sign-out endpoint exists yet; auth cookies are HTTP-only
        // and cannot be cleared from WebAssembly. Sign-out currently relies on the
        // access/refresh token cookies expiring on their own.
        return Task.CompletedTask;
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
}
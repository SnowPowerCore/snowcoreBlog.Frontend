using System.Net;
using Apizr;
using BitzArt.Blazor.Auth;
using BitzArt.Blazor.Auth.Server;
using Ixnas.AltchaNet;
using snowcoreBlog.Frontend.SharedComponents.Features.Antiforgery;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Extensions;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.Host.Services;

public class GlobalReaderAccountAuthenticationService : AuthenticationService<LoginByAssertionDto>
{
    private readonly IApizrManager<IReaderAccountManagementApi> _readerAccountApi;
    private readonly IApizrManager<ITokensApi> _tokensApi;
    private readonly AltchaSolver _altchaSolver;
    private readonly CookieContainer _managedCookieContainer;
    private readonly IStore _store;

    public GlobalReaderAccountAuthenticationService(IApizrManager<IReaderAccountManagementApi> readerAccountApi,
                                                    IApizrManager<ITokensApi> tokensApi,
                                                    AltchaSolver altchaSolver,
                                                    CookieContainer managedCookieContainer,
                                                    IStore store)
    {
        _readerAccountApi = readerAccountApi;
        _tokensApi = tokensApi;
        _altchaSolver = altchaSolver;
        _managedCookieContainer = managedCookieContainer;
        _store = store;
    }

    public override async Task<AuthenticationResult> SignInAsync(LoginByAssertionDto signInPayload, CancellationToken cancellationToken = default)
    {
        using var antiforgeryState = _store.GetState<AntiforgeryState>();
        if (antiforgeryState is default(AntiforgeryState))
            return AuthenticationResult.Failure("No antiforgery token");

        using var captchaResponse = await _tokensApi.ExecuteAsync(static (opt, api) => api.GetAltchaChallenge(opt), o => o.WithCancellation(cancellationToken));
        if (!captchaResponse.IsSuccess)
            return AuthenticationResult.Failure(captchaResponse.Exception?.Message);

        var solverResult = _altchaSolver.Solve(captchaResponse.Result);
        if (!solverResult.Success)
            return AuthenticationResult.Failure(solverResult.Error.Message);

        using var loginResponse = await _readerAccountApi.ExecuteAsync((opt, api) =>
            api.LoginByAssertion(signInPayload, antiforgeryState.RequestVerificationToken, solverResult.Altcha, opt), o => o.WithCancellation(cancellationToken));
        var loginData = loginResponse.ToData<LoginByAssertionResultDto>(out var loginErrors);
        if (loginData is default(LoginByAssertionResultDto) || loginErrors.Count > 0)
            return AuthenticationResult.Failure(loginErrors.FirstOrDefault());

        var responseCookies = _managedCookieContainer.GetAllCookies()?.Where(x => x.HttpOnly);
        var accessTokenCookie = responseCookies.FirstOrDefault(x => string.Equals(x.Name, ".DotNet.Application.User.SystemKey", StringComparison.OrdinalIgnoreCase));
        var refreshTokenCookie = responseCookies.FirstOrDefault(x => string.Equals(x.Name, ".DotNet.Application.User.SystemUpdateKey", StringComparison.OrdinalIgnoreCase));

        if (accessTokenCookie is default(Cookie) || refreshTokenCookie is default(Cookie))
            return AuthenticationResult.Failure("No cookie");

        return AuthenticationResult.Success(
            new(accessTokenCookie.Value, new(accessTokenCookie.Expires), refreshTokenCookie.Value, new(refreshTokenCookie.Expires)));
    }

    public override Task<AuthenticationResult> RefreshJwtPairAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AuthenticationResult.Success(new(string.Empty, DateTimeOffset.MinValue)));
    }
}
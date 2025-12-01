using System.Net.Http.Headers;
using Apizr;
using BitzArt.Blazor.Auth;
using BitzArt.Blazor.Auth.Server;
using Ixnas.AltchaNet;
using snowcoreBlog.Frontend.Core.Models.Cookie;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Extensions;
using CookieExtensions = snowcoreBlog.Frontend.Infrastructure.Extensions.CookieExtensions;

namespace snowcoreBlog.Frontend.Host.Services;

public class GlobalReaderAccountAuthenticationService : AuthenticationService<LoginByAssertionDto>
{
    private readonly IApizrManager<IReaderAccountManagementApi> _readerAccountApi;
    private readonly IApizrManager<IReaderAccountTokensApi> _tokensApi;
    private readonly AltchaSolver _altchaSolver;

    public GlobalReaderAccountAuthenticationService(IApizrManager<IReaderAccountManagementApi> readerAccountApi,
                                                    IApizrManager<IReaderAccountTokensApi> tokensApi,
                                                    AltchaSolver altchaSolver)
    {
        _readerAccountApi = readerAccountApi;
        _tokensApi = tokensApi;
        _altchaSolver = altchaSolver;
    }

    public override async Task<AuthenticationResult> SignInAsync(LoginByAssertionDto signInPayload, CancellationToken cancellationToken = default)
    {
        using var antiforgeryResponse = await _tokensApi.ExecuteAsync(static (opt, api) => api.GetAntiforgeryToken(opt), o => o.WithCancellation(cancellationToken));
        var antiforgeryData = antiforgeryResponse.ToData<AntiforgeryResultDto>(out var antiforgeryErrors);
        if (antiforgeryData is default(AntiforgeryResultDto) || antiforgeryErrors.Count > 0)
            return AuthenticationResult.Failure("Failed to retrieve antiforgery token");
        
        using var captchaResponse = await _tokensApi.ExecuteAsync(static (opt, api) => api.GetAltchaChallenge(opt), o => o.WithCancellation(cancellationToken));
        if (!captchaResponse.IsSuccess)
            return AuthenticationResult.Failure(captchaResponse.Exception?.Message);

        var solverResult = _altchaSolver.Solve(captchaResponse.Result);
        if (!solverResult.Success)
            return AuthenticationResult.Failure(solverResult.Error.Message);

        using var loginResponse = await _readerAccountApi.ExecuteAsync((opt, api) =>
            api.LoginByAssertion(signInPayload, antiforgeryData.RequestToken!, solverResult.Altcha, opt), o => o.WithCancellation(cancellationToken));
        var loginData = loginResponse.ToData<LoginByAssertionResultDto>(out var loginErrors);
        if (loginData is default(LoginByAssertionResultDto) || loginErrors.Count > 0)
            return AuthenticationResult.Failure(loginErrors.FirstOrDefault());

        // Extract cookies from the response headers
        if (loginResponse.ApiResponse?.Headers is default(HttpResponseHeaders) ||
            !loginResponse.ApiResponse.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            return AuthenticationResult.Failure("No authentication cookies in response");

        var cookie = default(CookieInfo);
        var accessTokenCookie = string.Empty;
        var refreshTokenCookie = string.Empty;
        var accessTokenExpiration = DateTimeOffset.UtcNow.AddHours(1); // Default expiration
        var refreshTokenExpiration = DateTimeOffset.UtcNow.AddHours(1); // Default expiration

        foreach (var setCookieHeader in setCookieHeaders)
        {
            if (setCookieHeader.StartsWith(".DotNet.Application.User.SystemKey"))
            {
                cookie = CookieExtensions.ParseSetCookieHeader(setCookieHeader);
                if (cookie is not default(CookieInfo))
                {
                    accessTokenCookie = cookie.Value;
                    accessTokenExpiration = cookie.Expires ?? accessTokenExpiration;
                }
            }
            else if (setCookieHeader.StartsWith(".DotNet.Application.User.SystemUpdateKey"))
            {
                cookie = CookieExtensions.ParseSetCookieHeader(setCookieHeader);
                if (cookie is not default(CookieInfo))
                {
                    refreshTokenCookie = cookie.Value;
                    refreshTokenExpiration = cookie.Expires ?? refreshTokenExpiration;
                }
            }
        }

        if (string.IsNullOrEmpty(accessTokenCookie) || string.IsNullOrEmpty(refreshTokenCookie))
            return AuthenticationResult.Failure("Missing required authentication cookies");

        return AuthenticationResult.Success(
            new(accessTokenCookie, accessTokenExpiration, refreshTokenCookie, refreshTokenExpiration));
    }

    public override Task<AuthenticationResult> RefreshJwtPairAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AuthenticationResult.Success(new(string.Empty, DateTimeOffset.MinValue)));
    }
}
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

public class GlobalReaderAccountAuthenticationService(IApizrManager<IReaderAccountManagementApi> readerAccountApi,
                                                IApizrManager<IReaderAccountTokensApi> tokensApi,
                                                IHttpClientFactory httpClientFactory,
                                                AltchaSolver altchaSolver) : AuthenticationService<LoginByAssertionDto>
{
    public override Task<AuthenticationResult> SignInAsync(LoginByAssertionDto signInPayload, CancellationToken cancellationToken = default)
    {
        // One-time code sign-in completes through the OTP verification endpoint;
        // passkey sign-in completes through the assertion endpoint.
        return string.IsNullOrWhiteSpace(signInPayload.OneTimeCode)
            ? SignInByAssertionAsync(signInPayload, cancellationToken)
            : SignInByOneTimeCodeAsync(signInPayload, cancellationToken);
    }

    private async Task<AuthenticationResult> SignInByAssertionAsync(LoginByAssertionDto signInPayload, CancellationToken cancellationToken = default)
    {
        using var antiforgeryResponse = await tokensApi.ExecuteAsync(static (opt, api) => api.GetAntiforgeryToken(opt), o => o.WithCancellation(cancellationToken));
        var antiforgeryData = antiforgeryResponse.ToData<AntiforgeryResultDto>(out var antiforgeryErrors);
        if (antiforgeryData is default(AntiforgeryResultDto) || antiforgeryErrors.Count > 0)
            return AuthenticationResult.Failure("Failed to retrieve antiforgery token");
        
        using var captchaResponse = await tokensApi.ExecuteAsync(static (opt, api) => api.GetAltchaChallenge(opt), o => o.WithCancellation(cancellationToken));
        if (!captchaResponse.IsSuccess)
            return AuthenticationResult.Failure(captchaResponse.Exception?.Message ?? "Failed to retrieve captcha challenge");

        var solverResult = altchaSolver.Solve(captchaResponse.Result);
        if (!solverResult.Success)
            return AuthenticationResult.Failure(solverResult.Error.Message);

        using var loginResponse = await readerAccountApi.ExecuteAsync((opt, api) =>
            api.LoginByAssertion(signInPayload, antiforgeryData.RequestToken!, solverResult.Altcha, opt), o => o.WithCancellation(cancellationToken));
        var loginData = loginResponse.ToData<LoginByAssertionResultDto>(out var loginErrors);
        if (loginData is default(LoginByAssertionResultDto) || loginErrors.Count > 0)
            return AuthenticationResult.Failure(loginErrors.FirstOrDefault() ?? "Login failed");

        // Extract cookies from the response headers
        if (loginResponse.ApiResponse?.Headers is default(HttpResponseHeaders) ||
            !loginResponse.ApiResponse.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            return AuthenticationResult.Failure("No authentication cookies in response");

        return BuildResultFromCookies(setCookieHeaders);
    }

    private async Task<AuthenticationResult> SignInByOneTimeCodeAsync(LoginByAssertionDto signInPayload, CancellationToken cancellationToken = default)
    {
        using var antiforgeryResponse = await tokensApi.ExecuteAsync(static (opt, api) => api.GetAntiforgeryToken(opt), o => o.WithCancellation(cancellationToken));
        var antiforgeryData = antiforgeryResponse.ToData<AntiforgeryResultDto>(out var antiforgeryErrors);
        if (antiforgeryData is default(AntiforgeryResultDto) || antiforgeryErrors.Count > 0)
            return AuthenticationResult.Failure("Failed to retrieve antiforgery token");

        using var captchaResponse = await tokensApi.ExecuteAsync(static (opt, api) => api.GetAltchaChallenge(opt), o => o.WithCancellation(cancellationToken));
        if (!captchaResponse.IsSuccess)
            return AuthenticationResult.Failure(captchaResponse.Exception?.Message ?? "Failed to retrieve captcha challenge");

        var solverResult = altchaSolver.Solve(captchaResponse.Result);
        if (!solverResult.Success)
            return AuthenticationResult.Failure(solverResult.Error.Message);

        using var verifyResponse = await readerAccountApi.ExecuteAsync((opt, api) =>
            api.VerifyOneTimeCode(new VerifyOneTimeCodeDto { Email = signInPayload.Email, Code = signInPayload.OneTimeCode! }, antiforgeryData.RequestToken!, solverResult.Altcha, opt), o => o.WithCancellation(cancellationToken));
        var verifyData = verifyResponse.ToData<LoginByOneTimeCodeResultDto>(out var verifyErrors);
        if (verifyData is default(LoginByOneTimeCodeResultDto) || verifyErrors.Count > 0)
            return AuthenticationResult.Failure(verifyErrors.FirstOrDefault() ?? "Login failed");

        if (verifyResponse.ApiResponse?.Headers is default(HttpResponseHeaders) ||
            !verifyResponse.ApiResponse.Headers.TryGetValues("Set-Cookie", out var verifyCookieHeaders))
            return AuthenticationResult.Failure("No authentication cookies in response");

        return BuildResultFromCookies(verifyCookieHeaders);
    }

    private static AuthenticationResult BuildResultFromCookies(IEnumerable<string> setCookieHeaders)
    {
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
        return RefreshInternalAsync(refreshToken, cancellationToken);
    }

    private async Task<AuthenticationResult> RefreshInternalAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("ReadersManagementClient");
        using var response = await client.PostAsJsonAsync("tokens/refresh/v1", refreshToken, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return AuthenticationResult.Failure($"Token refresh failed: {(int)response.StatusCode}");

        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            return AuthenticationResult.Failure("No authentication cookies in refresh response");

        return BuildResultFromCookies(setCookieHeaders);
    }
}
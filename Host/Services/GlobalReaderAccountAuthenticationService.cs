using Apizr;
using BitzArt.Blazor.Auth;
using BitzArt.Blazor.Auth.Server;
using Ixnas.AltchaNet;
using snowcoreBlog.Frontend.ReadersManagement.Features.Antiforgery;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Extensions;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.Host.Services;

public class GlobalReaderAccountAuthenticationService : AuthenticationService<LoginByAssertionDto>
{
    private readonly IApizrManager<IReaderAccountManagementApi> _readerAccountApi;
    private readonly IApizrManager<IReaderAccountTokensApi> _tokensApi;
    private readonly AltchaSolver _altchaSolver;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStore _store;

    public GlobalReaderAccountAuthenticationService(IApizrManager<IReaderAccountManagementApi> readerAccountApi,
                                                    IApizrManager<IReaderAccountTokensApi> tokensApi,
                                                    AltchaSolver altchaSolver,
                                                    IHttpContextAccessor httpContextAccessor,
                                                    IStore store)
    {
        _readerAccountApi = readerAccountApi;
        _tokensApi = tokensApi;
        _altchaSolver = altchaSolver;
        _httpContextAccessor = httpContextAccessor;
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

        // Get cookies from the HttpContext that were set by the API response
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return AuthenticationResult.Failure("No HTTP context");

        var accessTokenCookie = httpContext.Request.Cookies[".DotNet.Application.User.SystemKey"];
        var refreshTokenCookie = httpContext.Request.Cookies[".DotNet.Application.User.SystemUpdateKey"];

        if (string.IsNullOrEmpty(accessTokenCookie) || string.IsNullOrEmpty(refreshTokenCookie))
            return AuthenticationResult.Failure("No authentication cookies");

        // For expiration, use a default or extract from loginData if available
        var expiration = DateTimeOffset.UtcNow.AddHours(1); // Default expiration

        return AuthenticationResult.Success(
            new(accessTokenCookie, expiration, refreshTokenCookie, expiration));
    }

    public override Task<AuthenticationResult> RefreshJwtPairAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AuthenticationResult.Success(new(string.Empty, DateTimeOffset.MinValue)));
    }
}
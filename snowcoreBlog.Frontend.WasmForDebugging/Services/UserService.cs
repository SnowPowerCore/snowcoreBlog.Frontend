using System.Security.Claims;
using Apizr;
using BitzArt.Blazor.Auth;
using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Components.Authorization;
using snowcoreBlog.Frontend.SharedComponents.Features.Antiforgery;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Extensions;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.WasmForDebugging.Services;

public class UserService(IApizrManager<IReaderAccountManagementApi> readerAccountApi) : IUserService
{
    public async Task<AuthenticationState> GetAuthenticationStateAsync(CancellationToken cancellationToken = default)
    {
        using var response = await readerAccountApi.ExecuteAsync(static (opt, api) =>
            api.RequestAuthData(opt), o => o.WithCancellation(cancellationToken));
        var data = response.ToData<AuthenticationStateDto>(out var errors);
        if (data is default(AuthenticationStateDto) && errors.Count > 0)
            return new AuthenticationState(new ClaimsPrincipal());
        return new AuthenticationState(new(new ClaimsIdentity(data!.Claims.Select(x => new Claim(x.Key, x.Value)))));
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<AuthenticationOperationInfo> RefreshJwtPairAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AuthenticationOperationInfo());
    }
}

public class UserService<TSignInPayload>(IApizrManager<IReaderAccountManagementApi> readerAccountApi,
                                         IApizrManager<ITokensApi> tokensApi,
                                         AltchaSolver altchaSolver,
                                         IStore store)
    : UserService(readerAccountApi), IUserService<LoginByAssertionDto>
{
    public async Task<AuthenticationOperationInfo> SignInAsync(LoginByAssertionDto signInPayload, CancellationToken cancellationToken = default)
    {
        using var antiforgeryState = store.GetState<AntiforgeryState>();
        if (antiforgeryState is default(AntiforgeryState))
            return new AuthenticationOperationInfo(errorMessage: "No antiforgery token");

        using var captchaResponse = await tokensApi.ExecuteAsync(static (opt, api) => api.GetAltchaChallenge(opt), o => o.WithCancellation(cancellationToken));
        if (!captchaResponse.IsSuccess)
            return new AuthenticationOperationInfo(errorMessage: captchaResponse.Exception?.Message);

        var solverResult = altchaSolver.Solve(captchaResponse.Result);
        if (!solverResult.Success)
            return new AuthenticationOperationInfo(errorMessage: solverResult.Error.Message);

        using var response = await readerAccountApi.ExecuteAsync((opt, api) =>
            api.LoginByAssertion(signInPayload, antiforgeryState.RequestVerificationToken, solverResult.Altcha, opt), o => o.WithCancellation(cancellationToken));
        var data = response.ToData<LoginByAssertionResultDto>(out var errors);
        if (data is default(LoginByAssertionResultDto) || errors.Count > 0)
            return new AuthenticationOperationInfo(errorMessage: errors.FirstOrDefault());
        return new AuthenticationOperationInfo(true);
    }
}
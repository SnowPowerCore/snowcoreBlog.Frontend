using System.Net.Http.Json;
using System.Security.Claims;
using BitzArt.Blazor.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using snowcoreBlog.Frontend.Infrastructure.Extensions.Auth;
using snowcoreBlog.Frontend.Infrastructure.Models.Auth;

namespace snowcoreBlog.Frontend.Client.Services;

public class BlazorHostBackedClientUserService(HttpClient hostClient, NavigationManager navigator) : IUserService
{
    private protected readonly HttpClient HostClient = hostClient;

    public async Task<AuthenticationState> GetAuthenticationStateAsync(CancellationToken cancellationToken = default)
    {
        var response = await HostClient.GetFromJsonAsync<ClaimsPrincipalDto>($"{navigator.BaseUri}_auth/me", cancellationToken);

        if (response is default(ClaimsPrincipalDto))
            return new AuthenticationState(new ClaimsPrincipal());

        var principal = response.ToModel();

        return new AuthenticationState(principal);
    }

    public async Task<AuthenticationOperationInfo> RefreshJwtPairAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var response = await HostClient.PostAsJsonAsync($"{navigator.BaseUri}_auth/refresh", refreshToken, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return (await response.Content.ReadFromJsonAsync<AuthenticationOperationInfo>(cancellationToken))!;
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var response = await HostClient.PostAsJsonAsync($"{navigator.BaseUri}_auth/sign-out", default(object), cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}

internal class BlazorHostBackedClientUserService<TSignInPayload>(HttpClient hostClient, NavigationManager navigator)
    : BlazorHostBackedClientUserService(hostClient, navigator), IUserService<TSignInPayload>
{
    public async Task<AuthenticationOperationInfo> SignInAsync(TSignInPayload signInPayload, CancellationToken cancellationToken = default)
    {
        var response = await HostClient.PostAsJsonAsync($"{navigator.BaseUri}_auth/sign-in", signInPayload!, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return (await response.Content.ReadFromJsonAsync<AuthenticationOperationInfo>(cancellationToken))!;
    }
}
using System.Net.Http.Json;
using BitzArt.Blazor.Auth;
using Microsoft.AspNetCore.Components;

namespace snowcoreBlog.Frontend.Client.Services;

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

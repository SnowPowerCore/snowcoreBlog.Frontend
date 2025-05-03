using BitzArt.Blazor.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace snowcoreBlog.Frontend.WasmForDebugging.Providers;

public class WasmCookiesAuthenticationStateProvider(IUserService user) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var task = user.GetAuthenticationStateAsync();

        NotifyAuthenticationStateChanged(task);

        return await task;
    }
}
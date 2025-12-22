using System.Security.Claims;
using BitzArt.Blazor.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace snowcoreBlog.Frontend.Host.Providers;

public sealed class SafeAuthenticationStateProvider(IUserService userService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var authTask = userService.GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(authTask);
            return await authTask;
        }
        catch (ArgumentNullException ex) when (string.Equals(ex.ParamName, "token", StringComparison.OrdinalIgnoreCase) ||
                                             ex.Message.Contains("IDX10000", StringComparison.OrdinalIgnoreCase))
        {
            var state = Anonymous();
            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return state;
        }
        catch
        {
            var state = Anonymous();
            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return state;
        }
    }

    private static AuthenticationState Anonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}

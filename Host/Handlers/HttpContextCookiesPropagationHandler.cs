using snowcoreBlog.Frontend.Core.Models.Cookie;
using CookieExtensions = snowcoreBlog.Frontend.Infrastructure.Extensions.CookieExtensions;

namespace snowcoreBlog.Frontend.Host.Handlers;

public sealed class HttpContextCookiesPropagationHandler(IHttpContextAccessor httpContextAccessor, bool excludeAuthCookies = false) : DelegatingHandler
{
    private const string AntiforgeryCookiePrefix = ".AspNetCore.Antiforgery";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is default(HttpContext))
            return await base.SendAsync(request, cancellationToken);

        if (request.RequestUri is default(Uri))
            return await base.SendAsync(request, cancellationToken);

        // Add all cookies from the current HTTP request to the outgoing API request
        // This includes the antiforgery cookie that was set by the middleware
        foreach (var cookie in ctx.Request.Cookies)
        {
            // Skip authentication cookies if configured to exclude them
            if (excludeAuthCookies &&
                (cookie.Key == ".DotNet.Application.User.SystemKey" ||
                 cookie.Key == ".DotNet.Application.User.SystemUpdateKey"))
            {
                continue;
            }

            request.Headers.Add("Cookie", $"{cookie.Key}={cookie.Value}");
        }

        var response = await base.SendAsync(request, cancellationToken);

        // The backend rotates the antiforgery cookie on every token request
        // (GetAndStoreTokens), so the request token returned in the response body
        // only validates against the newly issued cookie. Forward that cookie to
        // the browser so the token/cookie pair stays consistent; otherwise the
        // browser keeps the stale cookie and the next POST fails validation.
        if (!ctx.Response.HasStarted &&
            response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            foreach (var setCookieHeader in setCookieHeaders)
            {
                var cookie = CookieExtensions.ParseSetCookieHeader(setCookieHeader);
                if (cookie is not default(CookieInfo) &&
                    cookie.Name.StartsWith(AntiforgeryCookiePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.Cookies.Append(cookie.Name, cookie.Value, new CookieOptions
                    {
                        Path = cookie.Path ?? "/",
                        Domain = cookie.Domain,
                        Secure = cookie.Secure,
                        HttpOnly = cookie.HttpOnly,
                        Expires = cookie.Expires,
                        SameSite = cookie.SameSite
                    });
                }
            }
        }

        return response;
    }
}
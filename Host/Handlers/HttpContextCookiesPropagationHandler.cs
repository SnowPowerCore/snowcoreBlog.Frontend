namespace snowcoreBlog.Frontend.Host.Handlers;

public sealed class HttpContextCookiesPropagationHandler(IHttpContextAccessor httpContextAccessor, bool excludeAuthCookies = false) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is default(HttpContext))
            return base.SendAsync(request, cancellationToken);

        if (request.RequestUri is default(Uri))
            return base.SendAsync(request, cancellationToken);

        // Add all cookies from the current HTTP request to the outgoing API request
        // This includes the antiforgery cookie that was set by the middleware
        foreach (var cookie in ctx.Request.Cookies)
        {
            // Skip authentication cookies if configured to exclude them
            // Also skip antiforgery cookies to force fresh token generation
            if (excludeAuthCookies &&
                (cookie.Key == ".DotNet.Application.User.SystemKey" ||
                 cookie.Key == ".DotNet.Application.User.SystemUpdateKey"))
            {
                continue;
            }

            request.Headers.Add("Cookie", $"{cookie.Key}={cookie.Value}");
        }

        return base.SendAsync(request, cancellationToken);
    }
}
namespace snowcoreBlog.Frontend.Host.Handlers;

public sealed class HttpContextCookiesPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly bool _excludeAuthCookies;

    public HttpContextCookiesPropagationHandler(IHttpContextAccessor httpContextAccessor, bool excludeAuthCookies = false)
    {
        _httpContextAccessor = httpContextAccessor;
        _excludeAuthCookies = excludeAuthCookies;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = _httpContextAccessor.HttpContext;
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
            if (_excludeAuthCookies &&
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
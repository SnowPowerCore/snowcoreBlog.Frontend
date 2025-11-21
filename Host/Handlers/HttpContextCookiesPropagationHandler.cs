namespace snowcoreBlog.Frontend.Host.Handlers;

public sealed class HttpContextCookiesPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCookiesPropagationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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
            request.Headers.Add("Cookie", $"{cookie.Key}={cookie.Value}");
        }

        return base.SendAsync(request, cancellationToken);
    }
}
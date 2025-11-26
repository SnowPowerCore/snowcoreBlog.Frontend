using snowcoreBlog.Frontend.Core.Models.Cookie;
using CookieExtensions = snowcoreBlog.Frontend.Infrastructure.Extensions.CookieExtensions;

namespace snowcoreBlog.Frontend.Host.Middleware;

public class ArticlesAntiforgeryInitMiddleware
{
    private const string HttpClientName = "ArticlesAntiforgeryClient";
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;

    public ArticlesAntiforgeryInitMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only initialize for initial page requests, not for SignalR or static assets
        if (context.Request.Path.StartsWithSegments("/_blazor") || 
            context.Request.Path.StartsWithSegments("/_framework") ||
            context.Request.Path.StartsWithSegments("/_content") ||
            context.Request.Path.Value?.Contains('.') == true) // Skip static files
        {
            await _next(context);
            return;
        }

        // Always fetch fresh antiforgery token for initial page loads
        // This ensures each new user session gets a fresh token
        try
        {
            // Create an HttpClient with the named client that includes cookie propagation
            // This allows the antiforgery token endpoint to receive the user's authentication state
            var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.GetAsync("https://localhost/api/articles/antiforgerytoken/v1");
            
            if (response.IsSuccessStatusCode)
            {
                // Extract Set-Cookie headers from the API response and forward them to the browser
                if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                {
                    foreach (var setCookieHeader in setCookieHeaders)
                    {
                        // Parse the Set-Cookie header
                        var cookie = CookieExtensions.ParseSetCookieHeader(setCookieHeader);
                        if (cookie is not default(CookieInfo))
                        {
                            context.Response.Cookies.Append(cookie.Name, cookie.Value, new CookieOptions
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
            }
        }
        catch { }

        await _next(context);
    }
}
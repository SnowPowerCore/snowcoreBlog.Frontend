using snowcoreBlog.Frontend.Core.Models.Cookie;
using CookieExtensions = snowcoreBlog.Frontend.Infrastructure.Extensions.CookieExtensions;

namespace snowcoreBlog.Frontend.Host.Middleware;

/// <summary>
/// Middleware that ensures fresh antiforgery cookies are set for the ReadersManagement service.
/// This runs early in the pipeline to establish cookies before any API calls are made.
/// </summary>
public class ReadersManagementAntiforgeryInitMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
{
    private const string HttpClientName = "ReadersManagementAntiforgeryClient";
    private const string CookiePrefix = ".AspNetCore.Antiforgery";

    public async Task InvokeAsync(HttpContext context)
    {
        // Only initialize for initial page requests, not for SignalR or static assets
        if (context.Request.Path.StartsWithSegments("/_blazor") || 
            context.Request.Path.StartsWithSegments("/_framework") ||
            context.Request.Path.StartsWithSegments("/_content") ||
            context.Request.Path.Value?.Contains('.') == true) // Skip static files
        {
            await next(context);
            return;
        }

        var hasAntiforgeryGookie = context.Request.Cookies.Any(c =>
            c.Key.StartsWith(CookiePrefix, StringComparison.OrdinalIgnoreCase));

        if (!hasAntiforgeryGookie)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient(HttpClientName);
                httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };
                httpClient.DefaultRequestHeaders.Pragma.Add(new System.Net.Http.Headers.NameValueHeaderValue("no-cache"));
                
                // Add a timestamp to force a unique request (cache-busting query param)
                var cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var response = await httpClient.GetAsync($"https://localhost/api/readers/antiforgerytoken/v1?_={cacheBuster}");
                
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
        }

        await next(context);
    }
}
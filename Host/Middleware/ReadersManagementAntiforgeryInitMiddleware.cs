using snowcoreBlog.Frontend.Core.Models.Cookie;
using CookieExtensions = snowcoreBlog.Frontend.Infrastructure.Extensions.CookieExtensions;

namespace snowcoreBlog.Frontend.Host.Middleware;

/// <summary>
/// Middleware that ensures fresh antiforgery cookies are set for the ReadersManagement service.
/// This runs early in the pipeline to establish cookies before any API calls are made.
/// </summary>
public class ReadersManagementAntiforgeryInitMiddleware(
    RequestDelegate next,
    IHttpClientFactory httpClientFactory,
    ILogger<ReadersManagementAntiforgeryInitMiddleware> logger)
{
    private const string HttpClientName = "ReadersManagementAntiforgeryClient";
    private const string MarkerCookieName = "af-init-readers";

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

        // Each backend service issues its own antiforgery cookie with a distinct
        // generated name, so a prefix check cannot tell which service issued an
        // existing cookie (an Articles cookie would wrongly suppress this init).
        // A dedicated marker cookie records that this service was initialized.
        var isInitialized = context.Request.Cookies.ContainsKey(MarkerCookieName);

        if (!isInitialized)
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
                                // Keep the original cookie name: the backend must receive it
                                // verbatim to validate the token/cookie pair.
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

                    context.Response.Cookies.Append(MarkerCookieName, "1", new CookieOptions
                    {
                        Path = "/",
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to initialize antiforgery cookie for ReadersManagement");
            }
        }

        await next(context);
    }
}
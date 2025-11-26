namespace snowcoreBlog.Frontend.Host.Middleware;

public class ApplyBasePathMiddleware
{
    private readonly RequestDelegate _next;

    public ApplyBasePathMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var basePath = context.Request.Headers["X-Forwarded-BasePath"].ToString();
        if (!string.IsNullOrEmpty(basePath))
        {
            context.Request.PathBase = basePath;
        }

        await _next(context);
    }
}
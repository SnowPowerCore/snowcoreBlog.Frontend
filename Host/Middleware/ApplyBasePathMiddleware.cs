namespace snowcoreBlog.Frontend.Host.Middleware;

public class ApplyBasePathMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var basePath = context.Request.Headers["X-Forwarded-BasePath"].ToString();
        if (!string.IsNullOrEmpty(basePath))
        {
            context.Request.PathBase = basePath;
        }

        await next(context);
    }
}
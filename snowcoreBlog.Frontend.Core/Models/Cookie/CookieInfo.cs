using Microsoft.AspNetCore.Http;

namespace snowcoreBlog.Frontend.Core.Models.Cookie;

public class CookieInfo
{
    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Path { get; set; }

    public string? Domain { get; set; }

    public bool Secure { get; set; }

    public bool HttpOnly { get; set; }

    public DateTimeOffset? Expires { get; set; }
    
    public SameSiteMode SameSite { get; set; }
}
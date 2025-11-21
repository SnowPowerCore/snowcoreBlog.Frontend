using Microsoft.AspNetCore.Http;
using snowcoreBlog.Frontend.Core.Models.Cookie;

namespace snowcoreBlog.Frontend.Infrastructure.Extensions;

public static class CookieExtensions
{
    public static CookieInfo? ParseSetCookieHeader(string setCookieHeader)
    {
        try
        {
            var parts = setCookieHeader.Split(';');
            if (parts.Length == 0) return default;

            var nameValue = parts[0].Split('=', 2);
            if (nameValue.Length != 2) return default;

            var cookie = new CookieInfo
            {
                Name = nameValue[0].Trim(),
                Value = nameValue[1].Trim(),
                Path = "/",
                SameSite = SameSiteMode.Lax
            };

            for (var i = 1; i < parts.Length; i++)
            {
                var attribute = parts[i].Trim();
                var attrParts = attribute.Split('=', 2);
                var attrName = attrParts[0].Trim().ToLowerInvariant();

                switch (attrName)
                {
                    case "path":
                        cookie.Path = attrParts.Length > 1 ? attrParts[1].Trim() : "/";
                        break;
                    case "domain":
                        cookie.Domain = attrParts.Length > 1 ? attrParts[1].Trim() : null;
                        break;
                    case "secure":
                        cookie.Secure = true;
                        break;
                    case "httponly":
                        cookie.HttpOnly = true;
                        break;
                    case "expires":
                        if (attrParts.Length > 1 && DateTimeOffset.TryParse(attrParts[1].Trim(), out var expires))
                        {
                            cookie.Expires = expires;
                        }
                        break;
                    case "max-age":
                        if (attrParts.Length > 1 && int.TryParse(attrParts[1].Trim(), out var maxAge))
                        {
                            cookie.Expires = DateTimeOffset.UtcNow.AddSeconds(maxAge);
                        }
                        break;
                    case "samesite":
                        if (attrParts.Length > 1)
                        {
                            cookie.SameSite = attrParts[1].Trim().ToLowerInvariant() switch
                            {
                                "strict" => SameSiteMode.Strict,
                                "lax" => SameSiteMode.Lax,
                                "none" => SameSiteMode.None,
                                _ => SameSiteMode.Lax
                            };
                        }
                        break;
                }
            }

            return cookie;
        }
        catch
        {
            return default;
        }
    }
}
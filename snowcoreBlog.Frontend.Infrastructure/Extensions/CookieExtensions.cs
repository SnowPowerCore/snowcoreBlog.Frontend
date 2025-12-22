using Microsoft.AspNetCore.Http;
using snowcoreBlog.Frontend.Core.Models.Cookie;

namespace snowcoreBlog.Frontend.Infrastructure.Extensions;

public static class CookieExtensions
{
    public static CookieInfo? ParseSetCookieHeader(string setCookieHeader)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(setCookieHeader))
                return default;

            ReadOnlySpan<char> input = setCookieHeader.AsSpan();
            var position = 0;

            scoped ReadOnlySpan<char> nameValueSegment = ReadNextSemicolonSegment(input, ref position);
            if (nameValueSegment.Length == 0)
                return default;

            nameValueSegment = nameValueSegment.Trim();
            if (nameValueSegment.Length == 0)
                return default;

            int equalsIndex = nameValueSegment.IndexOf('=');
            if (equalsIndex <= 0)
                return default;

            scoped ReadOnlySpan<char> nameSpan = nameValueSegment.Slice(0, equalsIndex).Trim();
            scoped ReadOnlySpan<char> valueSpan = nameValueSegment.Slice(equalsIndex + 1).Trim();
            if (nameSpan.Length == 0)
                return default;

            var cookie = new CookieInfo
            {
                Name = nameSpan.ToString(),
                Value = valueSpan.ToString(),
                Path = "/",
                SameSite = SameSiteMode.Lax
            };

            while (position < input.Length)
            {
                scoped ReadOnlySpan<char> attributeSegment = ReadNextSemicolonSegment(input, ref position);
                attributeSegment = attributeSegment.Trim();
                if (attributeSegment.Length == 0)
                    continue;

                scoped ReadOnlySpan<char> attrNameSpan;
                scoped ReadOnlySpan<char> attrValueSpan;

                int attributeEqualsIndex = attributeSegment.IndexOf('=');
                if (attributeEqualsIndex < 0)
                {
                    attrNameSpan = attributeSegment;
                    attrValueSpan = default;
                }
                else
                {
                    attrNameSpan = attributeSegment.Slice(0, attributeEqualsIndex).Trim();
                    attrValueSpan = attributeSegment.Slice(attributeEqualsIndex + 1).Trim();
                }

                if (attrNameSpan.Equals("path", StringComparison.OrdinalIgnoreCase))
                {
                    cookie.Path = attrValueSpan.Length > 0 ? attrValueSpan.ToString() : "/";
                }
                else if (attrNameSpan.Equals("domain", StringComparison.OrdinalIgnoreCase))
                {
                    cookie.Domain = attrValueSpan.Length > 0 ? attrValueSpan.ToString() : null;
                }
                else if (attrNameSpan.Equals("secure", StringComparison.OrdinalIgnoreCase))
                {
                    cookie.Secure = true;
                }
                else if (attrNameSpan.Equals("httponly", StringComparison.OrdinalIgnoreCase))
                {
                    cookie.HttpOnly = true;
                }
                else if (attrNameSpan.Equals("expires", StringComparison.OrdinalIgnoreCase))
                {
                    if (attrValueSpan.Length > 0 && DateTimeOffset.TryParse(attrValueSpan, out var expires))
                        cookie.Expires = expires;
                }
                else if (attrNameSpan.Equals("max-age", StringComparison.OrdinalIgnoreCase))
                {
                    if (attrValueSpan.Length > 0 && int.TryParse(attrValueSpan, out var maxAge))
                        cookie.Expires = DateTimeOffset.UtcNow.AddSeconds(maxAge);
                }
                else if (attrNameSpan.Equals("samesite", StringComparison.OrdinalIgnoreCase))
                {
                    if (attrValueSpan.Length > 0)
                    {
                        cookie.SameSite = attrValueSpan.Equals("strict", StringComparison.OrdinalIgnoreCase)
                            ? SameSiteMode.Strict
                            : attrValueSpan.Equals("lax", StringComparison.OrdinalIgnoreCase)
                                ? SameSiteMode.Lax
                                : attrValueSpan.Equals("none", StringComparison.OrdinalIgnoreCase)
                                    ? SameSiteMode.None
                                    : SameSiteMode.Lax;
                    }
                }
            }

            return cookie;
        }
        catch
        {
            return default;
        }
    }

    private static ReadOnlySpan<char> ReadNextSemicolonSegment(ReadOnlySpan<char> input, ref int position)
    {
        if (position >= input.Length)
            return default;

        var start = position;
        var nextSeparator = input.Slice(position).IndexOf(';');
        if (nextSeparator < 0)
        {
            position = input.Length;
            return input.Slice(start);
        }

        position += nextSeparator + 1;
        return input.Slice(start, nextSeparator);
    }
}
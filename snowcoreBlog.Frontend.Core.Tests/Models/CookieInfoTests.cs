using FluentAssertions;
using Microsoft.AspNetCore.Http;
using snowcoreBlog.Frontend.Core.Models.Cookie;

namespace snowcoreBlog.Frontend.Core.Tests.Models;

public class CookieInfoTests
{
    [Fact]
    public void CookieInfo_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var cookieInfo = new CookieInfo();

        // Assert
        cookieInfo.Name.Should().Be(string.Empty);
        cookieInfo.Value.Should().Be(string.Empty);
        cookieInfo.Path.Should().BeNull();
        cookieInfo.Domain.Should().BeNull();
        cookieInfo.Secure.Should().BeFalse();
        cookieInfo.HttpOnly.Should().BeFalse();
        cookieInfo.Expires.Should().BeNull();
    }

    [Fact]
    public void CookieInfo_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var expires = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var cookieInfo = new CookieInfo
        {
            Name = "auth_token",
            Value = "abc123",
            Path = "/",
            Domain = "example.com",
            Secure = true,
            HttpOnly = true,
            Expires = expires,
            SameSite = SameSiteMode.Strict
        };

        // Assert
        cookieInfo.Name.Should().Be("auth_token");
        cookieInfo.Value.Should().Be("abc123");
        cookieInfo.Path.Should().Be("/");
        cookieInfo.Domain.Should().Be("example.com");
        cookieInfo.Secure.Should().BeTrue();
        cookieInfo.HttpOnly.Should().BeTrue();
        cookieInfo.Expires.Should().Be(expires);
        cookieInfo.SameSite.Should().Be(SameSiteMode.Strict);
    }

    [Theory]
    [InlineData(SameSiteMode.None)]
    [InlineData(SameSiteMode.Lax)]
    [InlineData(SameSiteMode.Strict)]
    [InlineData(SameSiteMode.Unspecified)]
    public void CookieInfo_ShouldSupportAllSameSiteModes(SameSiteMode mode)
    {
        // Arrange & Act
        var cookieInfo = new CookieInfo
        {
            Name = "test",
            Value = "value",
            SameSite = mode
        };

        // Assert
        cookieInfo.SameSite.Should().Be(mode);
    }
}

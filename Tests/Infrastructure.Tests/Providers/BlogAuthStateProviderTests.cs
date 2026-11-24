using FluentAssertions;
using snowcoreBlog.Frontend.Infrastructure.Providers;

namespace snowcoreBlog.Frontend.Infrastructure.Tests.Providers;

public class BlogAuthStateProviderTests
{
    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnAnonymousUser()
    {
        // Arrange
        var provider = new BlogAuthStateProvider();

        // Act
        var authState = await provider.GetAuthenticationStateAsync();

        // Assert
        authState.Should().NotBeNull();
        authState.User.Should().NotBeNull();
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnUserWithNoIdentityName()
    {
        // Arrange
        var provider = new BlogAuthStateProvider();

        // Act
        var authState = await provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity!.Name.Should().BeNull();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnUserWithNoClaims()
    {
        // Arrange
        var provider = new BlogAuthStateProvider();

        // Act
        var authState = await provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Claims.Should().BeEmpty();
    }
}

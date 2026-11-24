using FluentAssertions;
using snowcoreBlog.Frontend.ServiceNotifications.Features.NotificationBar;

namespace snowcoreBlog.Frontend.ServiceNotifications.Tests.Features;

public class NotificationBarStateTests
{
    [Fact]
    public void Initialize_ShouldSetDefaultValues()
    {
        // Arrange
        var state = new NotificationBarState();

        // Act
        state.Initialize();

        // Assert
        state.ActiveNotifications.Should().BeEmpty();
        state.DismissedNotificationIds.Should().BeEmpty();
        state.IsLoading.Should().BeFalse();
        state.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void HasVisibleNotifications_WithNoNotifications_ShouldReturnFalse()
    {
        // Arrange
        var state = new NotificationBarState();
        state.Initialize();

        // Act
        var result = state.HasVisibleNotifications;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VisibleNotifications_WithNoNotifications_ShouldBeEmpty()
    {
        // Arrange
        var state = new NotificationBarState();
        state.Initialize();

        // Act
        var result = state.VisibleNotifications;

        // Assert
        result.Should().BeEmpty();
    }
}

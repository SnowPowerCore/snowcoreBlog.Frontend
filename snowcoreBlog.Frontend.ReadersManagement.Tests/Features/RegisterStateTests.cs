using FluentAssertions;
using snowcoreBlog.Frontend.ReadersManagement.Features.Register;

namespace snowcoreBlog.Frontend.ReadersManagement.Tests.Features;

public class RegisterStateTests
{
    [Fact]
    public void Initialize_ShouldSetDefaultValues()
    {
        // Arrange
        var state = new RegisterState();

        // Act
        state.Initialize();

        // Assert
        state.CurrentModel.Should().NotBeNull();
        state.CurrentModel.Email.Should().Be(string.Empty);
        state.CurrentModel.FirstName.Should().Be(string.Empty);
        state.CurrentModel.NickName.Should().Be(string.Empty);
        state.CurrentModel.InitialEmailConsent.Should().BeFalse();
        state.CurrentModel.ConfirmedAgreement.Should().BeFalse();
    }

    [Fact]
    public void CurrentModel_AfterInitialize_ShouldNotBeNull()
    {
        // Arrange
        var state = new RegisterState();

        // Act
        state.Initialize();

        // Assert
        state.CurrentModel.Should().NotBeNull();
    }
}

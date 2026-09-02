using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.ProfileCommands.CompleteOnboarding;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class CompleteOnboardingCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IProfileRepository> _profileRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly CompleteOnboardingCommandHandler _handler;

    public CompleteOnboardingCommandHandlerTests()
    {
        _userContextMock.Setup(c => c.UserId).Returns(UserId);
        _handler = new CompleteOnboardingCommandHandler(_profileRepoMock.Object, _userContextMock.Object);
    }

    private static Profile CreateProfile(bool hasCompletedOnboarding = false, string? preferredLanguage = null) => new()
    {
        AppUserId = UserId,
        HasCompletedOnboarding = hasCompletedOnboarding,
        PreferredLanguage = preferredLanguage,
    };

    [Fact]
    public async Task Handle_ValidLanguage_SetsOnboardingCompleteAndLanguage()
    {
        var profile = CreateProfile();
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _handler.Handle(new CompleteOnboardingCommand("ar"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        profile.HasCompletedOnboarding.Should().BeTrue();
        profile.PreferredLanguage.Should().Be("ar");
        _profileRepoMock.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullLanguage_CompletesOnboardingWithoutTouchingLanguage()
    {
        var profile = CreateProfile(preferredLanguage: "en");
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _handler.Handle(new CompleteOnboardingCommand(null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.HasCompletedOnboarding.Should().BeTrue();
        profile.PreferredLanguage.Should().Be("en");
    }

    [Fact]
    public async Task Handle_UnsupportedLanguage_CompletesOnboardingButIgnoresLanguage()
    {
        var profile = CreateProfile(preferredLanguage: "en");
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _handler.Handle(new CompleteOnboardingCommand("fr"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.HasCompletedOnboarding.Should().BeTrue();
        profile.PreferredLanguage.Should().Be("en");
    }

    [Fact]
    public async Task Handle_ProfileMissing_CreatesOneInsteadOfFailing()
    {
        // Some creation paths predate the guarantee that every AppUser gets a companion
        // Profile row - completing onboarding must not 404 a real, logged-in user over that.
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);

        Profile? added = null;
        _profileRepoMock.Setup(r => r.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .Callback<Profile, CancellationToken>((p, _) => added = p)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new CompleteOnboardingCommand("ar"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added.Should().NotBeNull();
        added!.AppUserId.Should().Be(UserId);
        added.HasCompletedOnboarding.Should().BeTrue();
        added.PreferredLanguage.Should().Be("ar");
        _profileRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

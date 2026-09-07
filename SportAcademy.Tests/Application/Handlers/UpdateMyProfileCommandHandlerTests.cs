using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.ProfileCommands.UpdateMyProfile;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Tests.Application.Handlers;

public class UpdateMyProfileCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IProfileRepository> _profileRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly UpdateMyProfileCommandHandler _handler;

    public UpdateMyProfileCommandHandlerTests()
    {
        _userContextMock.Setup(c => c.UserId).Returns(UserId);
        _userRepoMock.Setup(r => r.GetUserRoleAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(["Owner"]);

        _handler = new UpdateMyProfileCommandHandler(
            _userRepoMock.Object, _profileRepoMock.Object, _userContextMock.Object, _fileStorageMock.Object);
    }

    private static AppUser CreateUser(string? phoneNumber = null) => new()
    {
        Id = UserId,
        UserName = "jane",
        Email = "jane@example.com",
        PhoneNumber = phoneNumber,
        TenantId = TenantId,
        CreatedAt = DateTime.UtcNow,
    };

    private static Profile CreateProfile(string? imageUrl = null, string? bio = null) => new()
    {
        AppUserId = UserId,
        ProfileImageUrl = imageUrl,
        Bio = bio,
    };

    [Fact]
    public async Task Handle_PhoneNumberProvided_SetsItOnTheAppUser()
    {
        var user = CreateUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProfile());

        var result = await _handler.Handle(
            new UpdateMyProfileCommand("+96551234567", null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PhoneNumber.Should().Be("+96551234567");
        result.Data!.PhoneNumber.Should().Be("+96551234567");
        _userRepoMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullPhoneNumber_LeavesExistingPhoneUnchanged()
    {
        var user = CreateUser(phoneNumber: "+96551234567");
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProfile());

        var result = await _handler.Handle(new UpdateMyProfileCommand(null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PhoneNumber.Should().Be("+96551234567");
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NewProfileImageUrl_DeletesTheOldOneAndSetsTheNew()
    {
        var user = CreateUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var profile = CreateProfile(imageUrl: "/uploads/avatars/old.png");
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _handler.Handle(
            new UpdateMyProfileCommand(null, "/uploads/avatars/new.png", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.ProfileImageUrl.Should().Be("/uploads/avatars/new.png");
        result.Data!.ProfileImageUrl.Should().Be("/uploads/avatars/new.png");
        _fileStorageMock.Verify(f => f.DeleteImage("/uploads/avatars/old.png"), Times.Once);
    }

    [Fact]
    public async Task Handle_SameProfileImageUrlResubmitted_DoesNotDeleteIt()
    {
        var user = CreateUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var profile = CreateProfile(imageUrl: "/uploads/avatars/same.png");
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await _handler.Handle(
            new UpdateMyProfileCommand(null, "/uploads/avatars/same.png", null), CancellationToken.None);

        _fileStorageMock.Verify(f => f.DeleteImage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BioProvided_UpdatesTheProfile()
    {
        var user = CreateUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var profile = CreateProfile();
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _handler.Handle(
            new UpdateMyProfileCommand(null, null, "Head coach, 10 years experience."), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.Bio.Should().Be("Head coach, 10 years experience.");
        _profileRepoMock.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProfileMissing_CreatesOneInsteadOfFailing()
    {
        var user = CreateUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _profileRepoMock.Setup(r => r.GetByAppUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);

        Profile? added = null;
        _profileRepoMock.Setup(r => r.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .Callback<Profile, CancellationToken>((p, _) => added = p)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new UpdateMyProfileCommand(null, null, "New bio"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added.Should().NotBeNull();
        added!.AppUserId.Should().Be(UserId);
        added.Bio.Should().Be("New bio");
        _profileRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

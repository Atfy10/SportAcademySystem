using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Tests.Application.Handlers;

public class AdminCreateUserCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IProfileRepository> _profileRepoMock = new();
    private readonly AdminCreateUserCommandHandler _handler;

    public AdminCreateUserCommandHandlerTests()
    {
        _handler = new AdminCreateUserCommandHandler(_mapperMock.Object, _userRepoMock.Object, _profileRepoMock.Object);
    }

    private static AdminCreateUserCommand ValidCommand() =>
        new("jdoe", "jdoe@test.com", null, EmailConfirmed: false, IsActive: true);

    [Fact]
    public async Task Handle_ValidRequest_RegistersWithAPolicyCompliantPassword()
    {
        var command = ValidCommand();
        var mappedUser = new AppUser { UserName = command.UserName, Email = command.Email };

        _userRepoMock.Setup(r => r.IsUsernameExistAsync(command.UserName, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.IsEmailExistAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapperMock.Setup(m => m.Map<AppUser>(command)).Returns(mappedUser);

        string? capturedPassword = null;
        _userRepoMock
            .Setup(r => r.Register(mappedUser, It.IsAny<string>()))
            .Callback<AppUser, string>((_, password) => capturedPassword = password)
            .ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UserName.Should().Be(command.UserName);
        result.Data.GeneratedPassword.Should().Be(capturedPassword);

        // The concrete bug this locks in: a password that doesn't provably satisfy Identity's
        // policy must never be handed to Register - see SecurePasswordGeneratorTests for the
        // exhaustive check of the generator itself.
        capturedPassword.Should().MatchRegex("[A-Z]").And.MatchRegex("[a-z]").And.MatchRegex("[0-9]");
        capturedPassword!.Any(c => !char.IsLetterOrDigit(c)).Should().BeTrue();

        _profileRepoMock.Verify(r => r.AddAsync(
            It.Is<SportAcademy.Domain.Entities.Profile>(p => p.AppUserId == mappedUser.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UsernameAlreadyTaken_ReturnsFailureWithoutRegistering()
    {
        var command = ValidCommand();
        _userRepoMock.Setup(r => r.IsUsernameExistAsync(command.UserName, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        _userRepoMock.Verify(r => r.Register(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailAlreadyRegistered_ReturnsFailureWithoutRegistering()
    {
        var command = ValidCommand();
        _userRepoMock.Setup(r => r.IsUsernameExistAsync(command.UserName, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.IsEmailExistAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        _userRepoMock.Verify(r => r.Register(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IdentityRegistrationFails_ReturnsFailureWithErrors()
    {
        var command = ValidCommand();
        var mappedUser = new AppUser { UserName = command.UserName, Email = command.Email };

        _userRepoMock.Setup(r => r.IsUsernameExistAsync(command.UserName, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.IsEmailExistAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapperMock.Setup(m => m.Map<AppUser>(command)).Returns(mappedUser);
        _userRepoMock
            .Setup(r => r.Register(mappedUser, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "X", Description = "boom" }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        _profileRepoMock.Verify(r => r.AddAsync(It.IsAny<SportAcademy.Domain.Entities.Profile>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

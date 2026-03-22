using AutoMapper;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.BranchCommands.CreateBranch;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class CreateBranchCommandHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly CreateBranchCommandHandler _handler;

    public CreateBranchCommandHandlerTests()
    {
        _handler = new CreateBranchCommandHandler(_branchRepoMock.Object, _mapperMock.Object);
    }

    private static CreateBranchCommand CreateValidCommand(
        string name = "Main Branch",
        string city = "Riyadh",
        string country = "Saudi Arabia",
        string phoneNumber = "0501234567",
        string? email = "branch@academy.com",
        string coX = "24.7136",
        string coY = "46.6753") => new(
            Name: name,
            City: city,
            Country: country,
            PhoneNumber: phoneNumber,
            Email: email,
            CoX: coX,
            CoY: coY);

    private static Branch CreateMappedBranch(int id = 1) => new()
    {
        Id = id,
        Name = "Main Branch",
        City = "Riyadh",
        Country = "Saudi Arabia",
        PhoneNumber = "0501234567",
        Email = "branch@academy.com",
        CoX = "24.7136",
        CoY = "46.6753",
        IsActive = false
    };

    [Fact]
    public async Task Handle_ValidBranch_ReturnsSuccessAndSetsIsActive()
    {
        // Arrange
        var command = CreateValidCommand();
        var branch = CreateMappedBranch();

        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns(branch);
        _branchRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsCoordinatesExistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsPhoneNumberExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(1);
        branch.IsActive.Should().BeTrue();
        _branchRepoMock.Verify(r => r.AddAsync(branch, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsEmailExistException()
    {
        // Arrange
        var command = CreateValidCommand();
        var branch = CreateMappedBranch();

        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns(branch);
        _branchRepoMock.Setup(r => r.IsEmailExistAsync("branch@academy.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EmailExistException>();
    }

    [Fact]
    public async Task Handle_DuplicateCoordinates_ThrowsCoordinateExistException()
    {
        // Arrange
        var command = CreateValidCommand();
        var branch = CreateMappedBranch();

        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns(branch);
        _branchRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsCoordinatesExistAsync("24.7136", "46.6753", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<CoordinateExistException>();
    }

    [Fact]
    public async Task Handle_DuplicatePhoneNumber_ThrowsPhoneExistException()
    {
        // Arrange
        var command = CreateValidCommand();
        var branch = CreateMappedBranch();

        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns(branch);
        _branchRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsCoordinatesExistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsPhoneNumberExistAsync("0501234567", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<PhoneExistException>();
    }

    [Fact]
    public async Task Handle_NullEmail_TreatedAsEmptyString()
    {
        // Arrange
        var command = CreateValidCommand(email: null);
        var branch = CreateMappedBranch();
        branch.Email = null;

        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns(branch);
        _branchRepoMock.Setup(r => r.IsEmailExistAsync("", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsCoordinatesExistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsPhoneNumberExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _branchRepoMock.Verify(r => r.IsEmailExistAsync("", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MapperReturnsNull_ThrowsAutoMapperMappingException()
    {
        // Arrange
        var command = CreateValidCommand();
        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns((Branch?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<AutoMapperMappingException>();
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = CreateValidCommand();
        var branch = CreateMappedBranch();
        var cts = new CancellationTokenSource();

        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns(branch);
        _branchRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsCoordinatesExistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsPhoneNumberExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()))
            .Callback(() => cts.Token.ThrowIfCancellationRequested())
            .Returns(Task.CompletedTask);

        cts.Cancel();

        // Act & Assert
        var act = () => _handler.Handle(command, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_ValidationOrder_EmailBeforeCoordinates()
    {
        // Arrange
        var command = CreateValidCommand();
        var branch = CreateMappedBranch();
        var callOrder = new List<string>();

        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns(branch);
        _branchRepoMock.Setup(r => r.IsEmailExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("email"))
            .ReturnsAsync(true);
        _branchRepoMock.Setup(r => r.IsCoordinatesExistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("coords"))
            .ReturnsAsync(false);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EmailExistException>();
        callOrder.Should().Equal("email");
    }

    [Theory]
    [InlineData("test1@example.com")]
    [InlineData("test2@example.com")]
    public async Task Handle_VariousEmails_ChecksCorrectEmail(string email)
    {
        // Arrange
        var command = CreateValidCommand(email: email);
        var branch = CreateMappedBranch();
        branch.Email = email;

        _mapperMock.Setup(m => m.Map<Branch>(command)).Returns(branch);
        _branchRepoMock.Setup(r => r.IsEmailExistAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsCoordinatesExistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.IsPhoneNumberExistAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _branchRepoMock.Setup(r => r.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _branchRepoMock.Verify(r => r.IsEmailExistAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }
}

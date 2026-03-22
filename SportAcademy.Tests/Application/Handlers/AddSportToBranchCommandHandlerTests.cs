using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.BranchCommands.AddSportToBranch;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class AddSportToBranchCommandHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<ISportRepository> _sportRepoMock = new();
    private readonly Mock<ISportBranchRepository> _sportBranchRepoMock = new();
    private readonly AddSportToBranchCommandHandler _handler;

    public AddSportToBranchCommandHandlerTests()
    {
        _handler = new AddSportToBranchCommandHandler(
            _branchRepoMock.Object,
            _sportRepoMock.Object,
            _sportBranchRepoMock.Object);
    }

    private static AddSportToBranchCommand CreateValidCommand(int sportId = 1, int branchId = 1) =>
        new(SportId: sportId, BranchId: branchId);

    private static Branch CreateBranch(int id = 1) => new()
    {
        Id = id,
        Name = "Main Branch",
        City = "Riyadh",
        Country = "Saudi Arabia",
        PhoneNumber = "0501234567",
        Email = "branch@academy.com",
        CoX = "24.7136",
        CoY = "46.6753",
        IsActive = true
    };

    private static Sport CreateSport(int id = 1) => new()
    {
        Id = id,
        Name = "Swimming",
        Description = "Swimming lessons"
    };

    [Fact]
    public async Task Handle_ValidSportAndBranch_CreatesSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand(1, 1);
        var branch = CreateBranch(1);
        var sport = CreateSport(1);

        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(branch);
        _sportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sport);
        _sportBranchRepoMock.Setup(r => r.IsExistAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _sportBranchRepoMock.Setup(r => r.AddAsync(It.IsAny<SportBranch>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("Sport added to branch successfully.");
        result.Message.Should().Be(OperationType.Add.ToString());
        _sportBranchRepoMock.Verify(r => r.AddAsync(It.IsAny<SportBranch>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BranchNotFound_ThrowsBranchNotFoundException()
    {
        // Arrange
        var command = CreateValidCommand(1, 999);

        _branchRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Branch?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BranchNotFoundException>();
    }

    [Fact]
    public async Task Handle_SportNotFound_ThrowsSportNotFoundException()
    {
        // Arrange
        var command = CreateValidCommand(999, 1);
        var branch = CreateBranch(1);

        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(branch);
        _sportRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Sport?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<SportNotFoundException>();
    }

    [Fact]
    public async Task Handle_SportAlreadyAddedToBranch_ThrowsConflictException()
    {
        // Arrange
        var command = CreateValidCommand(1, 1);
        var branch = CreateBranch(1);
        var sport = CreateSport(1);

        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(branch);
        _sportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sport);
        _sportBranchRepoMock.Setup(r => r.IsExistAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    [InlineData(5, 10)]
    public async Task Handle_DifferentSportAndBranchIds_CreatesCorrectAssociation(int sportId, int branchId)
    {
        // Arrange
        var command = CreateValidCommand(sportId, branchId);
        var branch = CreateBranch(branchId);
        var sport = CreateSport(sportId);

        _branchRepoMock.Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>())).ReturnsAsync(branch);
        _sportRepoMock.Setup(r => r.GetByIdAsync(sportId, It.IsAny<CancellationToken>())).ReturnsAsync(sport);
        _sportBranchRepoMock.Setup(r => r.IsExistAsync(sportId, branchId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _sportBranchRepoMock.Setup(r => r.AddAsync(It.IsAny<SportBranch>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _sportBranchRepoMock.Verify(
            r => r.AddAsync(It.Is<SportBranch>(sb => sb.SportId == sportId && sb.BranchId == branchId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = CreateValidCommand(1, 1);
        var branch = CreateBranch(1);
        var sport = CreateSport(1);
        var cts = new CancellationTokenSource();

        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(branch);
        _sportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sport);
        _sportBranchRepoMock.Setup(r => r.IsExistAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _sportBranchRepoMock.Setup(r => r.AddAsync(It.IsAny<SportBranch>(), It.IsAny<CancellationToken>()))
            .Callback(() => cts.Token.ThrowIfCancellationRequested())
            .Returns(Task.CompletedTask);

        cts.Cancel();

        // Act & Assert
        var act = () => _handler.Handle(command, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}

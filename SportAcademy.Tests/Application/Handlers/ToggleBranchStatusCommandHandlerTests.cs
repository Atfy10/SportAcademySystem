using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.BranchCommands.ToggleBranchStatus;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class ToggleBranchStatusCommandHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly ToggleBranchStatusCommandHandler _handler;

    public ToggleBranchStatusCommandHandlerTests()
    {
        _handler = new ToggleBranchStatusCommandHandler(_branchRepoMock.Object);
    }

    private static ToggleBranchStatusCommand CreateValidCommand(int id = 1) => new(Id: id);

    [Fact]
    public async Task Handle_BranchExists_TogglesStatus()
    {
        // Arrange
        var command = CreateValidCommand(1);

        _branchRepoMock.Setup(r => r.ToggleIsActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be(OperationType.Update.ToString());
    }

    [Fact]
    public async Task Handle_BranchNotFound_ThrowsBranchNotFoundException()
    {
        // Arrange
        var command = CreateValidCommand(999);

        _branchRepoMock.Setup(r => r.ToggleIsActiveAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BranchNotFoundException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Handle_DifferentBranchIds_ToggleCorrectBranch(int branchId)
    {
        // Arrange
        var command = CreateValidCommand(branchId);

        _branchRepoMock.Setup(r => r.ToggleIsActiveAsync(branchId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _branchRepoMock.Verify(r => r.ToggleIsActiveAsync(branchId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

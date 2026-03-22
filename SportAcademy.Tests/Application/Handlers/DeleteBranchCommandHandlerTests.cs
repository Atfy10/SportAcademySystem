using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.BranchCommands.DeleteBranch;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class DeleteBranchCommandHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly DeleteBranchCommandHandler _handler;

    public DeleteBranchCommandHandlerTests()
    {
        _handler = new DeleteBranchCommandHandler(_branchRepoMock.Object);
    }

    private static DeleteBranchCommand CreateValidCommand(int id = 1) => new(Id: id);

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

    [Fact]
    public async Task Handle_ValidBranch_DeletesSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var branch = CreateBranch(1);

        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(branch);
        _branchRepoMock.Setup(r => r.DeleteAsync(branch, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be(OperationType.Delete.ToString());
        _branchRepoMock.Verify(r => r.DeleteAsync(branch, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BranchNotFound_ThrowsBranchNotFoundException()
    {
        // Arrange
        var command = CreateValidCommand(999);

        _branchRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Branch?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BranchNotFoundException>();
    }

    [Fact]
    public async Task Handle_CancellationBeforeDelete_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var branch = CreateBranch(1);
        var cts = new CancellationTokenSource();

        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(branch);
        _branchRepoMock.Setup(r => r.DeleteAsync(branch, It.IsAny<CancellationToken>()))
            .Callback(() => cts.Token.ThrowIfCancellationRequested())
            .Returns(Task.CompletedTask);

        cts.Cancel();

        // Act & Assert
        var act = () => _handler.Handle(command, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Handle_DifferentBranchIds_DeletesCorrectBranch(int branchId)
    {
        // Arrange
        var command = CreateValidCommand(branchId);
        var branch = CreateBranch(branchId);

        _branchRepoMock.Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>())).ReturnsAsync(branch);
        _branchRepoMock.Setup(r => r.DeleteAsync(branch, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _branchRepoMock.Verify(r => r.DeleteAsync(branch, It.IsAny<CancellationToken>()), Times.Once);
    }
}

using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.DeleteEnrollment;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class DeleteEnrollmentCommandHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly DeleteEnrollmentCommandHandler _handler;

    public DeleteEnrollmentCommandHandlerTests()
    {
        _handler = new DeleteEnrollmentCommandHandler(_enrollmentRepoMock.Object);
    }

    private static DeleteEnrollmentCommand CreateValidCommand(int id = 1) => new(Id: id);

    private static Enrollment CreateEnrollment(int id = 1) => new()
    {
        Id = id,
        TraineeId = 1,
        TraineeGroupId = 1,
        SubscriptionDetailsId = 1,
        EnrollmentDate = DateTime.UtcNow,
        ExpiryDate = DateTime.UtcNow.AddMonths(1),
        SessionAllowed = 8,
        SessionRemaining = 4,
        IsActive = true
    };

    [Fact]
    public async Task Handle_ValidEnrollment_DeletesSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.DeleteAsync(enrollment, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be(OperationType.Delete.ToString());
        _enrollmentRepoMock.Verify(r => r.DeleteAsync(enrollment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EnrollmentNotFound_ThrowsEnrollmentNotFoundException()
    {
        // Arrange
        var command = CreateValidCommand(999);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EnrollmentNotFoundException>();
    }

    [Fact]
    public async Task Handle_CancellationBeforeDelete_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1);
        var cts = new CancellationTokenSource();

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.DeleteAsync(enrollment, It.IsAny<CancellationToken>()))
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
    public async Task Handle_DifferentEnrollmentIds_DeletesCorrectEnrollment(int enrollmentId)
    {
        // Arrange
        var command = CreateValidCommand(enrollmentId);
        var enrollment = CreateEnrollment(enrollmentId);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.DeleteAsync(enrollment, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _enrollmentRepoMock.Verify(r => r.DeleteAsync(enrollment, It.IsAny<CancellationToken>()), Times.Once);
    }
}

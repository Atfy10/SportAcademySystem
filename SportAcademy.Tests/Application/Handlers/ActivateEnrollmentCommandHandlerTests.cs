using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.ActivateEnrollment;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class ActivateEnrollmentCommandHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly ActivateEnrollmentCommandHandler _handler;

    public ActivateEnrollmentCommandHandlerTests()
    {
        _handler = new ActivateEnrollmentCommandHandler(_enrollmentRepoMock.Object);
    }

    private static ActivateEnrollmentCommand CreateValidCommand(int enrollmentId = 1) =>
        new(Id: enrollmentId);

    private static Enrollment CreateEnrollment(int id = 1, bool isActive = false) => new()
    {
        Id = id,
        TraineeId = 1,
        TraineeGroupId = 1,
        SubscriptionDetailsId = 1,
        EnrollmentDate = DateTime.UtcNow,
        ExpiryDate = DateTime.UtcNow.AddMonths(1),
        SessionAllowed = 8,
        SessionRemaining = 8,
        IsActive = isActive
    };

    [Fact]
    public async Task Handle_ValidEnrollmentId_ActivatesAndReturnsSuccess()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, isActive: false);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be(OperationType.Update.ToString());
        enrollment.IsActive.Should().BeTrue();
        _enrollmentRepoMock.Verify(r => r.UpdateAsync(enrollment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EnrollmentNotFound_ThrowsIdNotFoundException()
    {
        // Arrange
        var command = CreateValidCommand(999);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task Handle_AlreadyActive_SetsToActiveAgain()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, isActive: true);

        Enrollment? capturedEnrollment = null;
        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((e, ct) => capturedEnrollment = e)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedEnrollment!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, isActive: false);
        var cancellationTokenSource = new CancellationTokenSource();

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback(() => cancellationTokenSource.Token.ThrowIfCancellationRequested())
            .Returns(Task.CompletedTask);

        cancellationTokenSource.Cancel();

        // Act & Assert
        var act = () => _handler.Handle(command, cancellationTokenSource.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Handle_DifferentEnrollmentIds_FetchesAndUpdatesCorrectOne(int enrollmentId)
    {
        // Arrange
        var command = CreateValidCommand(enrollmentId);
        var enrollment = CreateEnrollment(enrollmentId, isActive: false);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _enrollmentRepoMock.Verify(
            r => r.UpdateAsync(enrollment, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, isActive: false);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

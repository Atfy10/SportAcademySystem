using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.SuspendEnrollment;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class SuspendEnrollmentCommandHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly SuspendEnrollmentCommandHandler _handler;

    public SuspendEnrollmentCommandHandlerTests()
    {
        _handler = new SuspendEnrollmentCommandHandler(
            _enrollmentRepoMock.Object,
            new Mock<IUserContextService>().Object,
            new Mock<IUserRepository>().Object,
            new Mock<MediatR.IPublisher>().Object);
    }

    private static SuspendEnrollmentCommand CreateValidCommand(int enrollmentId = 1) =>
        new(Id: enrollmentId);

    private static Enrollment CreateEnrollment(int id = 1, EnrollmentStatus status = EnrollmentStatus.Active) => new()
    {
        Id = id,
        TraineeId = 1,
        TraineeGroupId = 1,
        SubscriptionDetailsId = 1,
        EnrollmentDate = DateTime.UtcNow,
        ExpiryDate = DateTime.UtcNow.AddMonths(1),
        SessionAllowed = 8,
        SessionRemaining = 4,
        Status = status
    };

    [Fact]
    public async Task Handle_ValidEnrollmentId_SuspendsAndReturnsSuccess()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Active);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be($"{OperationType.Update} operation done successfully");
        enrollment.Status.Should().Be(EnrollmentStatus.Suspended);
        _enrollmentRepoMock.Verify(r => r.UpdateAsync(enrollment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EnrollmentNotFound_ThrowsIdNotFoundException()
    {
        // Arrange
        var command = CreateValidCommand(999);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task Handle_AlreadySuspended_StaysSuspended()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Suspended);

        Enrollment? capturedEnrollment = null;
        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((e, ct) => capturedEnrollment = e)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedEnrollment!.Status.Should().Be(EnrollmentStatus.Suspended);
    }

    [Fact]
    public async Task Handle_ExpiredEnrollment_ThrowsEnrollmentAlreadyExpiredException()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Active);
        enrollment.ExpiryDate = DateTime.UtcNow.AddDays(-1);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EnrollmentAlreadyExpiredException>();
        _enrollmentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EndedEnrollment_ThrowsEnrollmentAlreadyExpiredException()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Ended);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EnrollmentAlreadyExpiredException>();
        _enrollmentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveEnrollment_BecomesSuspended()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Active);

        Enrollment? capturedEnrollment = null;
        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((e, ct) => capturedEnrollment = e)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedEnrollment!.Status.Should().Be(EnrollmentStatus.Suspended);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Active);
        var cancellationTokenSource = new CancellationTokenSource();

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
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
    [InlineData(10)]
    [InlineData(50)]
    public async Task Handle_DifferentEnrollmentIds_SuspendsCorrectOne(int enrollmentId)
    {
        // Arrange
        var command = CreateValidCommand(enrollmentId);
        var enrollment = CreateEnrollment(enrollmentId, EnrollmentStatus.Active);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
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
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Active);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_PreservesSessions_SuspensionDoesntAffectSessionData()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Active);
        enrollment.SessionAllowed = 8;
        enrollment.SessionRemaining = 3;
        var originalExpiryDate = enrollment.ExpiryDate;

        Enrollment? capturedEnrollment = null;
        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((e, ct) => capturedEnrollment = e)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - ExpiryDate/SessionRemaining are deliberately untouched by Suspend.
        capturedEnrollment!.SessionAllowed.Should().Be(8);
        capturedEnrollment.SessionRemaining.Should().Be(3);
        capturedEnrollment.ExpiryDate.Should().Be(originalExpiryDate);
        capturedEnrollment.Status.Should().Be(EnrollmentStatus.Suspended);
    }
}

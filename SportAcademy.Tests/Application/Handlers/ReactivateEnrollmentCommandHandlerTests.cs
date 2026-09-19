using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.ReactivateEnrollment;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class ReactivateEnrollmentCommandHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly Mock<ITraineeGroupRepository> _groupRepoMock = new();
    private readonly ReactivateEnrollmentCommandHandler _handler;

    public ReactivateEnrollmentCommandHandlerTests()
    {
        _handler = new ReactivateEnrollmentCommandHandler(
            _enrollmentRepoMock.Object,
            _groupRepoMock.Object,
            new Mock<IUserContextService>().Object,
            new Mock<IUserRepository>().Object,
            new Mock<MediatR.IPublisher>().Object);
    }

    private static ReactivateEnrollmentCommand CreateValidCommand(int enrollmentId = 1, int sessionRemaining = 4) =>
        new(Id: enrollmentId, SessionRemaining: sessionRemaining);

    private static Enrollment CreateEnrollment(int id = 1, EnrollmentStatus status = EnrollmentStatus.Suspended, int sessionAllowed = 8) => new()
    {
        Id = id,
        TraineeId = 1,
        TraineeGroupId = 1,
        SubscriptionDetailsId = 1,
        EnrollmentDate = DateTime.UtcNow.AddMonths(-1),
        ExpiryDate = DateTime.UtcNow.AddDays(-3),
        SessionAllowed = sessionAllowed,
        SessionRemaining = sessionAllowed,
        Status = status
    };

    private static TraineeGroup CreateGroupWithSchedule(int id = 1) => new()
    {
        Id = id,
        GroupSchedules = [new GroupSchedule { Id = 1, TraineeGroupId = id, Day = DateTime.UtcNow.DayOfWeek }],
    };

    [Fact]
    public async Task Handle_SuspendedEnrollment_ReactivatesAndRecomputesExpiry()
    {
        // Arrange
        var command = CreateValidCommand(1, sessionRemaining: 4);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Suspended);
        var group = CreateGroupWithSchedule(1);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _groupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        enrollment.SessionRemaining.Should().Be(4);
        enrollment.ExpiryDate.Should().BeOnOrAfter(DateTime.UtcNow.Date);
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
    public async Task Handle_EnrollmentNotSuspended_ThrowsEnrollmentNotSuspendedException()
    {
        // Arrange
        var command = CreateValidCommand(1);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Active);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EnrollmentNotSuspendedException>();
    }

    [Fact]
    public async Task Handle_SessionRemainingExceedsAllowed_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var command = CreateValidCommand(1, sessionRemaining: 20);
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Suspended, sessionAllowed: 8);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Handle_DifferentEnrollmentIds_FetchesAndUpdatesCorrectOne(int enrollmentId)
    {
        // Arrange
        var command = CreateValidCommand(enrollmentId, sessionRemaining: 3);
        var enrollment = CreateEnrollment(enrollmentId, EnrollmentStatus.Suspended);
        var group = CreateGroupWithSchedule(1);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _groupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
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
        var enrollment = CreateEnrollment(1, EnrollmentStatus.Suspended);
        var group = CreateGroupWithSchedule(1);

        _enrollmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _groupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _enrollmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

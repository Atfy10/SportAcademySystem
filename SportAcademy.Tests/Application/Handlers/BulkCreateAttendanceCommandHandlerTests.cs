using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.AttendanceCommands.BulkCreateAttendance;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class BulkCreateAttendanceCommandHandlerTests
{
    private readonly Mock<IAttendanceRepository> _attendanceRepoMock = new();
    private readonly Mock<ISessionOccurrenceRepository> _sessionRepoMock = new();
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly BulkCreateAttendanceCommandHandler _handler;

    public BulkCreateAttendanceCommandHandlerTests()
    {
        _handler = new BulkCreateAttendanceCommandHandler(
            _attendanceRepoMock.Object,
            _sessionRepoMock.Object,
            _enrollmentRepoMock.Object,
            _publisherMock.Object);
    }

    private static BulkCreateAttendanceCommand CreateValidCommand(List<AttendanceItem> items) =>
        new(Items: items);

    private static AttendanceItem CreateAttendanceItem(
        int sessionId = 1,
        int traineeId = 1,
        AttendanceStatus status = AttendanceStatus.Present,
        string? checkInTime = null) => new(
            SessionOccurrenceId: sessionId,
            TraineeId: traineeId,
            Status: status,
            CheckInTime: checkInTime);

    private static Attendance CreateAttendance(int id = 1, int enrollmentId = 1) => new()
    {
        Id = id,
        EnrollmentId = enrollmentId,
        SessionOccurrenceId = 1,
        AttendanceStatus = AttendanceStatus.Present,
        AttendanceDate = DateTime.UtcNow,
        CheckInTime = TimeOnly.FromDateTime(DateTime.UtcNow),
        CoachNote = ""
    };

    [Fact]
    public async Task Handle_EmptyList_ReturnsSuccess()
    {
        // Arrange
        var command = CreateValidCommand(new List<AttendanceItem>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Add operation done successfully");
    }

    [Fact]
    public async Task Handle_SingleNewAttendance_CreatesSuccessfully()
    {
        // Arrange
        var item = CreateAttendanceItem(1, 1, AttendanceStatus.Present, "10:30");
        var command = CreateValidCommand(new List<AttendanceItem> { item });

        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _attendanceRepoMock.Setup(r => r.GetBySessionAndTraineeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync((Attendance?)null);
        _attendanceRepoMock.Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _attendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SkipsNullGroupId_ContinuesToNext()
    {
        // Arrange
        var item1 = CreateAttendanceItem(1, 1, AttendanceStatus.Present);
        var item2 = CreateAttendanceItem(2, 2, AttendanceStatus.Present);
        var command = CreateValidCommand(new List<AttendanceItem> { item1, item2 });

        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((int?)null);
        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(2, 2, It.IsAny<CancellationToken>())).ReturnsAsync(6);
        _attendanceRepoMock.Setup(r => r.GetBySessionAndTraineeAsync(2, 2, It.IsAny<CancellationToken>())).ReturnsAsync((Attendance?)null);
        _attendanceRepoMock.Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _attendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SkipsNullEnrollmentId_ContinuesToNext()
    {
        // Arrange
        var item1 = CreateAttendanceItem(1, 1, AttendanceStatus.Present);
        var item2 = CreateAttendanceItem(2, 2, AttendanceStatus.Present);
        var command = CreateValidCommand(new List<AttendanceItem> { item1, item2 });

        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync((int?)null);
        
        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(2, 2, It.IsAny<CancellationToken>())).ReturnsAsync(6);
        _attendanceRepoMock.Setup(r => r.GetBySessionAndTraineeAsync(2, 2, It.IsAny<CancellationToken>())).ReturnsAsync((Attendance?)null);
        _attendanceRepoMock.Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _attendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateExistingAttendance_CallsUpdate()
    {
        // Arrange
        var item = CreateAttendanceItem(1, 1, AttendanceStatus.Late, "10:45");
        var command = CreateValidCommand(new List<AttendanceItem> { item });
        var existingAttendance = CreateAttendance(1, 5);

        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _attendanceRepoMock.Setup(r => r.GetBySessionAndTraineeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(existingAttendance);
        _attendanceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingAttendance.AttendanceStatus.Should().Be(AttendanceStatus.Late);
        existingAttendance.CheckInTime.Should().Be(TimeOnly.Parse("10:45"));
        _attendanceRepoMock.Verify(r => r.UpdateAsync(existingAttendance, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullCheckInTime_DefaultsToNow()
    {
        // Arrange
        var item = CreateAttendanceItem(1, 1, AttendanceStatus.Present, null);
        var command = CreateValidCommand(new List<AttendanceItem> { item });

        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _attendanceRepoMock.Setup(r => r.GetBySessionAndTraineeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync((Attendance?)null);
        _attendanceRepoMock.Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _attendanceRepoMock.Verify(
            r => r.AddAsync(It.Is<Attendance>(a => a.CheckInTime.Hour >= 0 && a.CheckInTime.Hour <= 23), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MixedCreateAndUpdate_HandlesCorrectly()
    {
        // Arrange
        var newItem = CreateAttendanceItem(1, 1, AttendanceStatus.Present);
        var updateItem = CreateAttendanceItem(2, 2, AttendanceStatus.Late, "11:00");
        var command = CreateValidCommand(new List<AttendanceItem> { newItem, updateItem });
        var existingAttendance = CreateAttendance(2, 6);

        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(2, 2, It.IsAny<CancellationToken>())).ReturnsAsync(6);
        _attendanceRepoMock.Setup(r => r.GetBySessionAndTraineeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync((Attendance?)null);
        _attendanceRepoMock.Setup(r => r.GetBySessionAndTraineeAsync(2, 2, It.IsAny<CancellationToken>())).ReturnsAsync(existingAttendance);
        _attendanceRepoMock.Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _attendanceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _attendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
        _attendanceRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_VariousStatuses_ProcessesCorrectly()
    {
        // Arrange
        var items = new List<AttendanceItem>
        {
            CreateAttendanceItem(1, 1, AttendanceStatus.Present),
            CreateAttendanceItem(2, 2, AttendanceStatus.Late),
            CreateAttendanceItem(3, 3, AttendanceStatus.Absent)
        };
        var command = CreateValidCommand(items);

        foreach (var item in items)
        {
            _sessionRepoMock.Setup(r => r.GetTraineeGroupIdAsync(item.SessionOccurrenceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item.TraineeId);
            _enrollmentRepoMock.Setup(r => r.GetEnrollmentIdAsync(item.TraineeId, item.TraineeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item.TraineeId + 10);
            _attendanceRepoMock.Setup(r => r.GetBySessionAndTraineeAsync(item.SessionOccurrenceId, item.TraineeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Attendance?)null);
        }

        _attendanceRepoMock.Setup(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _attendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}

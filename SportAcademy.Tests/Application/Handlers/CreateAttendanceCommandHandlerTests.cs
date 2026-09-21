using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.AttendanceExceptions;

namespace SportAcademy.Tests.Application.Handlers;

// The attendance window: from the session's start until midnight at the end of that same day.
// Fixed dates and a fixed tenant clock, so nothing here depends on when the suite runs.
public class CreateAttendanceCommandHandlerTests
{
    private static readonly DateTime SessionStart = new(2026, 9, 21, 17, 30, 0);

    private readonly Mock<IAttendanceRepository> _attendanceRepoMock = new();
    private readonly Mock<ISessionOccurrenceRepository> _sessionRepoMock = new();
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ITenantClock> _tenantClockMock = new();
    private readonly CreateAttendanceCommandHandler _handler;

    public CreateAttendanceCommandHandlerTests()
    {
        _sessionRepoMock
            .Setup(r => r.GetTimingAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TraineeGroupId: 7, StartDateTime: SessionStart, DurationInMinutes: 60, Status: SessionStatus.Scheduled));
        _enrollmentRepoMock
            .Setup(r => r.GetEnrollmentIdAsync(5, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(99);
        _attendanceRepoMock
            .Setup(r => r.GetBySessionAndTraineeAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);
        _attendanceRepoMock
            .Setup(r => r.AddAsyncWithoutSave(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance a, CancellationToken _) => a);

        _handler = new CreateAttendanceCommandHandler(
            _attendanceRepoMock.Object,
            _sessionRepoMock.Object,
            _enrollmentRepoMock.Object,
            _unitOfWorkMock.Object,
            _publisherMock.Object,
            _tenantClockMock.Object);
    }

    private void SetNow(DateTime tenantNow) =>
        _tenantClockMock
            .Setup(c => c.GetLocalNowAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantNow);

    private static CreateAttendanceCommand Mark() =>
        new(SessionOccurrenceId: 1, TraineeId: 5, Status: AttendanceStatus.Present, CheckInTime: null);

    [Theory]
    [InlineData(17, 30, 0)]  // the moment the session starts
    [InlineData(19, 0, 0)]   // right after it ends
    [InlineData(22, 15, 0)]  // 3+ hours after it ended - the old 90-minute grace would have refused this
    [InlineData(23, 59, 59)] // last second of the day
    public async Task Handle_FromSessionStartUntilMidnight_RecordsAttendance(int hour, int minute, int second)
    {
        SetNow(new DateTime(2026, 9, 21, hour, minute, second));

        var result = await _handler.Handle(Mark(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _attendanceRepoMock.Verify(
            r => r.AddAsyncWithoutSave(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(2026, 9, 21, 17, 29, 59)] // one second before the session starts
    [InlineData(2026, 9, 21, 9, 0, 0)]    // that morning
    [InlineData(2026, 9, 22, 0, 0, 0)]    // midnight sharp: the window has closed
    [InlineData(2026, 9, 22, 8, 0, 0)]    // next morning
    [InlineData(2026, 9, 23, 17, 30, 0)]  // days later
    public async Task Handle_OutsideTheWindow_ThrowsWindowClosedAndRecordsNothing(
        int year, int month, int day, int hour, int minute, int second)
    {
        SetNow(new DateTime(year, month, day, hour, minute, second));

        var act = () => _handler.Handle(Mark(), CancellationToken.None);

        await act.Should().ThrowAsync<AttendanceWindowClosedException>()
            .WithMessage("*midnight*same day*");
        _attendanceRepoMock.Verify(
            r => r.AddAsyncWithoutSave(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Commands.AttendanceCommands.BulkCreateAttendance;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AttendanceCommands.BulkCreateAttendance;

public class BulkCreateAttendanceCommandHandler(
    IAttendanceRepository attendanceRepository,
    ISessionOccurrenceRepository sessionOccurrenceRepository,
    IEnrollmentRepository enrollmentRepository,
    IPublisher publisher)
    : IRequestHandler<BulkCreateAttendanceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        BulkCreateAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        var updatedSessionIds = new HashSet<int>();

        foreach (var item in request.Items)
        {
            // Excused must go through an ExcuseRequest for Owner/Admin approval, never written
            // directly - same rule CreateAttendanceCommandHandler enforces for the single-mark
            // endpoint. Skipped (not thrown) so one disallowed row doesn't fail the whole batch,
            // matching this handler's existing best-effort stance on bad rows below.
            if (item.Status == AttendanceStatus.Excused) continue;

            var timing = await sessionOccurrenceRepository.GetTimingAsync(
                item.SessionOccurrenceId, cancellationToken);
            if (timing == null) continue;

            // No marking attendance more than 15 minutes after the session ended.
            if (DateTime.Now > timing.Value.StartDateTime.AddMinutes(timing.Value.DurationInMinutes + 15))
                continue;

            var groupId = timing.Value.TraineeGroupId;

            var enrollmentId = await enrollmentRepository.GetEnrollmentIdAsync(
                item.TraineeId, groupId, cancellationToken);
            if (enrollmentId == null) continue;

            var attendance = await attendanceRepository.GetBySessionAndTraineeAsync(
                item.SessionOccurrenceId, item.TraineeId, cancellationToken);

            if (attendance == null)
            {
                var checkInTime = item.CheckInTime != null
                    ? TimeOnly.Parse(item.CheckInTime)
                    : TimeOnly.FromDateTime(DateTime.UtcNow);

                attendance = new Attendance
                {
                    EnrollmentId = enrollmentId.Value,
                    SessionOccurrenceId = item.SessionOccurrenceId,
                    AttendanceStatus = item.Status,
                    AttendanceDate = DateTime.UtcNow,
                    CheckInTime = checkInTime,
                    CoachNote = string.Empty
                };
                await attendanceRepository.AddAsync(attendance, cancellationToken);
            }
            else
            {
                if (item.CheckInTime != null)
                    attendance.CheckInTime = TimeOnly.Parse(item.CheckInTime);
                attendance.AttendanceStatus = item.Status;
                attendance.UpdatedAt = DateTime.UtcNow;
                await attendanceRepository.UpdateAsync(attendance, cancellationToken);
            }

            updatedSessionIds.Add(item.SessionOccurrenceId);
        }

        await publisher.Publish(
            new BulkAttendanceCreatedEvent(updatedSessionIds), cancellationToken);

        return Result<bool>.Success(true, OperationType.Add.ToString());
    }
}

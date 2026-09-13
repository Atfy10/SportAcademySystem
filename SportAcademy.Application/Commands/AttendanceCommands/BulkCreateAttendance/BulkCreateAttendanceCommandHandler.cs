using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Commands.AttendanceCommands.BulkCreateAttendance;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AttendanceCommands.BulkCreateAttendance;

public class BulkCreateAttendanceCommandHandler(
    IAttendanceRepository attendanceRepository,
    ISessionOccurrenceRepository sessionOccurrenceRepository,
    IEnrollmentRepository enrollmentRepository,
    IPublisher publisher,
    ITenantClock tenantClock)
    : IRequestHandler<BulkCreateAttendanceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        BulkCreateAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        var updatedSessionIds = new HashSet<int>();

        // timing.StartDateTime is a tenant wall-clock value (see CreateAttendanceCommandHandler's
        // identical concern) - resolved once per batch, not per item, since every item in one
        // bulk-mark call belongs to the same tenant/request.
        var tenantNow = await tenantClock.GetLocalNowAsync(cancellationToken);

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

            // Attendance can only be recorded from when the session starts until 120 minutes
            // after it ends - not before it starts either, since there's nothing to attend yet.
            if (tenantNow < timing.Value.StartDateTime
                || tenantNow > timing.Value.StartDateTime.AddMinutes(timing.Value.DurationInMinutes + 120))
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
                    : TimeOnly.FromDateTime(tenantNow);

                attendance = new Attendance
                {
                    EnrollmentId = enrollmentId.Value,
                    SessionOccurrenceId = item.SessionOccurrenceId,
                    AttendanceStatus = item.Status,
                    AttendanceDate = tenantNow,
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

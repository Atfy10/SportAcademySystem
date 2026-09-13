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
        // Rows that couldn't be saved for a real reason (not the deliberate Excused skip below) -
        // without this, the handler used to swallow every one of these and still report overall
        // success, so a coach marking attendance outside the allowed window (or for a bad row)
        // got a "saved" toast while nothing was actually recorded. See CLAUDE.md §6.
        var skipped = new List<string>();

        // timing.StartDateTime is a tenant wall-clock value (see CreateAttendanceCommandHandler's
        // identical concern) - resolved once per batch, not per item, since every item in one
        // bulk-mark call belongs to the same tenant/request.
        var tenantNow = await tenantClock.GetLocalNowAsync(cancellationToken);

        foreach (var item in request.Items)
        {
            // Excused must go through an ExcuseRequest for Owner/Admin approval, never written
            // directly - same rule CreateAttendanceCommandHandler enforces for the single-mark
            // endpoint. Skipped (not thrown) so one disallowed row doesn't fail the whole batch -
            // this one is deliberate, not an error, so it never joins `skipped` below.
            if (item.Status == AttendanceStatus.Excused) continue;

            var timing = await sessionOccurrenceRepository.GetTimingAsync(
                item.SessionOccurrenceId, cancellationToken);
            if (timing == null)
            {
                skipped.Add($"Trainee {item.TraineeId}: session not found.");
                continue;
            }

            // Attendance can only be recorded from when the session starts until 120 minutes
            // after it ends - not before it starts either, since there's nothing to attend yet.
            if (tenantNow < timing.Value.StartDateTime
                || tenantNow > timing.Value.StartDateTime.AddMinutes(timing.Value.DurationInMinutes + 120))
            {
                skipped.Add($"Trainee {item.TraineeId}: outside the attendance window (session start through 120 minutes after it ends).");
                continue;
            }

            var groupId = timing.Value.TraineeGroupId;

            var enrollmentId = await enrollmentRepository.GetEnrollmentIdAsync(
                item.TraineeId, groupId, cancellationToken);
            if (enrollmentId == null)
            {
                skipped.Add($"Trainee {item.TraineeId}: not enrolled in this group.");
                continue;
            }

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

        if (skipped.Count > 0)
        {
            return Result<bool>.Failure(
                OperationType.Add.ToString(),
                $"{skipped.Count} of {request.Items.Count} attendance record(s) could not be saved.",
                400,
                new Dictionary<string, string[]> { ["items"] = [.. skipped] });
        }

        return Result<bool>.Success(true, OperationType.Add.ToString());
    }
}

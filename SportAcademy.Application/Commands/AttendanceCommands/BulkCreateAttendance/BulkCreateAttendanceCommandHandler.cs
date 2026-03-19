using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Commands.AttendanceCommands.BulkCreateAttendance;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AttendanceCommands.BulkCreateAttendance;

public class BulkCreateAttendanceCommandHandler(
    IAttendanceRepository attendanceRepository,
    ISessionOccurrenceRepository sessionOccurrenceRepository,
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<BulkCreateAttendanceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        BulkCreateAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var groupId = await sessionOccurrenceRepository.GetTraineeGroupIdAsync(
                item.SessionOccurrenceId, cancellationToken);
            if (groupId == null) continue;

            var enrollmentId = await enrollmentRepository.GetEnrollmentIdAsync(
                item.TraineeId, groupId.Value, cancellationToken);
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
        }

        return Result<bool>.Success(true, OperationType.Add.ToString());
    }
}

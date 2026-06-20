using MediatR;
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

                attendance = Attendance.Create(
                    enrollmentId.Value,
                    item.SessionOccurrenceId,
                    item.Status,
                    DateTime.UtcNow,
                    checkInTime,
                    string.Empty);
                await attendanceRepository.AddAsync(attendance, cancellationToken);
            }
            else
            {
                attendance.UpdateStatus(item.Status, item.CheckInTime);
                await attendanceRepository.UpdateAsync(attendance, cancellationToken);
            }

            updatedSessionIds.Add(item.SessionOccurrenceId);
        }

        await publisher.Publish(
            new BulkAttendanceCreatedEvent(updatedSessionIds), cancellationToken);

        return Result<bool>.Success(true, OperationType.Add.ToString());
    }
}

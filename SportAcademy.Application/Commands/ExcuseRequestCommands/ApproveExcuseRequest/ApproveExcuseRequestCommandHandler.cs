using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
using SportAcademy.Domain.Exceptions.ExcuseRequestExceptions;

namespace SportAcademy.Application.Commands.ExcuseRequestCommands.ApproveExcuseRequest;

public class ApproveExcuseRequestCommandHandler(
    IExcuseRequestRepository excuseRequestRepository,
    IEnrollmentRepository enrollmentRepository,
    IAttendanceRepository attendanceRepository,
    IUserContextService userContext,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : IRequestHandler<ApproveExcuseRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ApproveExcuseRequestCommand request, CancellationToken cancellationToken)
    {
        var excuseRequest = await excuseRequestRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ExcuseRequestNotFoundException(request.Id.ToString());

        if (excuseRequest.Status != ExcuseRequestStatus.Pending)
            throw new ExcuseRequestAlreadyReviewedException(excuseRequest.Id, excuseRequest.Status.ToString());

        var enrollment = await enrollmentRepository.GetByIdWithGroupAndSubscriptionAsync(excuseRequest.EnrollmentId, cancellationToken)
            ?? throw new EnrollmentNotFoundException(excuseRequest.EnrollmentId.ToString());

        // Defer the trainee's subscription by one session-equivalent period rather than just
        // granting a spare session credit: Enrollment.ExpiryDate is a hard mirror of
        // SubscriptionDetails.EndDate everywhere else in this system (see
        // CreateEnrollmentCommandHandler/CreateSubscriptionDetailsCommandHandler), so extending
        // only the enrollment would immediately diverge from that rule. Extending the
        // subscription's own end date and re-syncing ExpiryDate from it keeps both consistent.
        // "One session" = however many days this group's own weekly cadence needs to fit one
        // more meeting (a group meeting twice a week only needs ~3-4 days; once a week needs 7).
        var weeklyFrequency = enrollment.TraineeGroup.GroupSchedules.Count;
        if (weeklyFrequency <= 0) weeklyFrequency = 1;
        var extensionDays = (int)Math.Ceiling(7.0 / weeklyFrequency);

        enrollment.SubscriptionDetails.EndDate = enrollment.SubscriptionDetails.EndDate.AddDays(extensionDays);
        enrollment.ExpiryDate = enrollment.SubscriptionDetails.EndDate.ToDateTime(TimeOnly.MinValue);
        await enrollmentRepository.UpdateAsyncWithoutSave(enrollment, cancellationToken);

        // Same idempotent upsert CreateAttendanceCommandHandler uses - if the coach already
        // marked something for this trainee/session (e.g. Absent, before filing the excuse),
        // approval overwrites it with Excused rather than erroring.
        var attendance = await attendanceRepository.GetBySessionAndTraineeAsync(
            excuseRequest.SessionOccurrenceId, excuseRequest.TraineeId, cancellationToken);
        if (attendance is null)
        {
            attendance = new Attendance
            {
                EnrollmentId = excuseRequest.EnrollmentId,
                SessionOccurrenceId = excuseRequest.SessionOccurrenceId,
                AttendanceStatus = AttendanceStatus.Excused,
                AttendanceDate = DateTime.UtcNow,
                CheckInTime = TimeOnly.FromDateTime(DateTime.UtcNow),
                CoachNote = excuseRequest.Reason
            };
            await attendanceRepository.AddAsyncWithoutSave(attendance, cancellationToken);
        }
        else
        {
            attendance.AttendanceStatus = AttendanceStatus.Excused;
            attendance.CoachNote = excuseRequest.Reason;
            attendance.UpdatedAt = DateTime.UtcNow;
            await attendanceRepository.UpdateAsyncWithoutSave(attendance, cancellationToken);
        }

        excuseRequest.Status = ExcuseRequestStatus.Approved;
        excuseRequest.ReviewedByUserId = userContext.UserId?.ToString();
        excuseRequest.ReviewedAt = DateTime.UtcNow;
        await excuseRequestRepository.UpdateAsyncWithoutSave(excuseRequest, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new ExcuseRequestReviewedEvent(excuseRequest.Id, Approved: true), cancellationToken);

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

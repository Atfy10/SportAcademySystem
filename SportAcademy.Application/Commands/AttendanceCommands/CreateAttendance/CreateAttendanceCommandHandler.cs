using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.AttendanceExceptions;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Result<int>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly ITenantClock _tenantClock;
        private readonly string _operation = OperationType.Add.ToString();

        public CreateAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            ISessionOccurrenceRepository sessionOccurrenceRepository,
            IEnrollmentRepository enrollmentRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher,
            ITenantClock tenantClock)
        {
            _attendanceRepository = attendanceRepository;
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
            _enrollmentRepository = enrollmentRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _tenantClock = tenantClock;
        }

        public async Task<Result<int>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
        {
            if (request.Status == AttendanceStatus.Excused)
                throw new ExcusedRequiresApprovalException();

            var timing = await _sessionOccurrenceRepository.GetTimingAsync(
                request.SessionOccurrenceId, cancellationToken)
                ?? throw new SessionOccurrenceNotFoundException(request.SessionOccurrenceId.ToString());

            // Checked before the time window, not instead of it: a session can be
            // Completed/Canceled/CancelledTemporary either by SessionOccurrenceCompletionService
            // (once its window closes) or by staff marking it by hand early, before the window
            // itself would have caught it.
            if (timing.Status != SessionStatus.Scheduled)
                throw new SessionNotScheduledException(request.SessionOccurrenceId, timing.Status.ToString());

            // timing.StartDateTime is written (session generation) and compared here as the
            // tenant's own wall-clock time, never converted to real UTC - "now" has to be
            // resolved the same way, or this comparison silently mixes two different clocks for
            // any tenant not in UTC.
            var tenantNow = await _tenantClock.GetLocalNowAsync(cancellationToken);

            // Attendance can only be recorded from when the session starts until midnight at the
            // end of that same day - not before it starts either, since there's nothing to attend
            // yet. See AttendanceWindow.
            if (!AttendanceWindow.IsOpen(tenantNow, timing.StartDateTime))
                throw new AttendanceWindowClosedException(request.SessionOccurrenceId);

            var groupId = timing.TraineeGroupId;

            var enrollmentId = await _enrollmentRepository.GetEnrollmentIdAsync(
                request.TraineeId, groupId, cancellationToken)
                ?? throw new EnrollmentNotFoundException(
                    $"trainee {request.TraineeId} in group {groupId}");

            var checkInTime = request.CheckInTime != null
                ? TimeOnly.Parse(request.CheckInTime)
                : TimeOnly.FromDateTime(tenantNow);

            // Idempotent, same as bulk create: marking an already-recorded trainee again
            // updates the existing row instead of throwing a duplicate-key error.
            var attendance = await _attendanceRepository.GetBySessionAndTraineeAsync(
                request.SessionOccurrenceId, request.TraineeId, cancellationToken);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EnrollmentId = enrollmentId,
                    SessionOccurrenceId = request.SessionOccurrenceId,
                    AttendanceStatus = request.Status,
                    AttendanceDate = tenantNow,
                    CheckInTime = checkInTime,
                    CoachNote = string.Empty
                };
                await _attendanceRepository.AddAsyncWithoutSave(attendance, cancellationToken);
            }
            else
            {
                attendance.AttendanceStatus = request.Status;
                attendance.CheckInTime = checkInTime;
                attendance.UpdatedAt = DateTime.UtcNow;
                await _attendanceRepository.UpdateAsyncWithoutSave(attendance, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new AttendanceCreatedEvent(request.SessionOccurrenceId), cancellationToken);

            return Result<int>.Success(attendance.Id, _operation);
        }
    }
}

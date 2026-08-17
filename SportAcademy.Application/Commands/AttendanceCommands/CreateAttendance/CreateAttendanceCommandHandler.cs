using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Result<int>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly string _operation = OperationType.Add.ToString();

        public CreateAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            ISessionOccurrenceRepository sessionOccurrenceRepository,
            IEnrollmentRepository enrollmentRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _attendanceRepository = attendanceRepository;
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
            _enrollmentRepository = enrollmentRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var groupId = await _sessionOccurrenceRepository.GetTraineeGroupIdAsync(
                request.SessionOccurrenceId, cancellationToken)
                ?? throw new SessionOccurrenceNotFoundException(request.SessionOccurrenceId.ToString());

            var enrollmentId = await _enrollmentRepository.GetEnrollmentIdAsync(
                request.TraineeId, groupId, cancellationToken)
                ?? throw new EnrollmentNotFoundException(
                    $"trainee {request.TraineeId} in group {groupId}");

            var checkInTime = request.CheckInTime != null
                ? TimeOnly.Parse(request.CheckInTime)
                : TimeOnly.FromDateTime(DateTime.UtcNow);

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
                    AttendanceDate = DateTime.UtcNow,
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

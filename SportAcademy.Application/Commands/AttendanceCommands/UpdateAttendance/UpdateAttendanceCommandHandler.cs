using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.AttendanceExceptions;

namespace SportAcademy.Application.Commands.AttendanceCommands.UpdateAttendance
{
    public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, Result<AttendanceDto>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly string _operation = OperationType.Update.ToString();

        public UpdateAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<Result<AttendanceDto>> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new AttendanceNotFoundException($"{request.Id}");

            attendance.UpdateDetails(request.CoachNote, request.EnrollmentId, request.SessionOccurrenceId);

            cancellationToken.ThrowIfCancellationRequested();

            await _attendanceRepository.UpdateAsync(attendance, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var attendanceDto = attendance.ToDto();

            return Result<AttendanceDto>.Success(attendanceDto, _operation);
        }
    }
}

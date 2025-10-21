using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.AttendanceCommands.DeleteAttendance
{
    public class DeleteAttendanceCommandHandler
        : IRequestHandler<DeleteAttendanceCommand, Result<bool>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private static readonly string _operation = OperationType.Delete.ToString();

        public DeleteAttendanceCommandHandler(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<Result<bool>> Handle(DeleteAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new AttendanceNotFoundException(request.Id.ToString());

            await _attendanceRepository.DeleteAsync(attendance, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}

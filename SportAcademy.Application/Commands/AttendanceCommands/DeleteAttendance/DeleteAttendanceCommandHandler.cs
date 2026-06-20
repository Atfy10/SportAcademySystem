using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.AttendanceExceptions;

namespace SportAcademy.Application.Commands.AttendanceCommands.DeleteAttendance
{
    public class DeleteAttendanceCommandHandler
        : IRequestHandler<DeleteAttendanceCommand, Result<bool>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IPublisher _publisher;
        private static readonly string _operation = OperationType.Delete.ToString();

        public DeleteAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            IPublisher publisher)
        {
            _attendanceRepository = attendanceRepository;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(DeleteAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new AttendanceNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            await _attendanceRepository.DeleteAsync(attendance, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _publisher.Publish(new AttendanceDeletedEvent(), cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}

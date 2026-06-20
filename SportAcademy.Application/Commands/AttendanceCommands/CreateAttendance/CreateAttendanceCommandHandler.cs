using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Result<int>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IPublisher _publisher;
        private readonly string _operation = OperationType.Add.ToString();

        public CreateAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            IPublisher publisher)
        {
            _attendanceRepository = attendanceRepository;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendanceEntity = request.ToAttendance();

            cancellationToken.ThrowIfCancellationRequested();

            await _attendanceRepository.AddAsync(attendanceEntity, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _publisher.Publish(new AttendanceCreatedEvent(request.SessionOccurrenceId), cancellationToken);

            return Result<int>.Success(attendanceEntity.Id, _operation);
        }
    }
}

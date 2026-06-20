using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Result<int>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;
        private readonly IPublisher _publisher;
        private readonly string _operation = OperationType.Add.ToString();

        public CreateAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            IMapper mapper,
            IPublisher publisher)
        {
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendanceEntity = _mapper.Map<Attendance>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            cancellationToken.ThrowIfCancellationRequested();

            await _attendanceRepository.AddAsync(attendanceEntity, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _publisher.Publish(new AttendanceCreatedEvent(request.SessionOccurrenceId), cancellationToken);

            return Result<int>.Success(attendanceEntity.Id, _operation);
        }
    }
}

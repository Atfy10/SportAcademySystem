using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.AttendanceExceptions;

namespace SportAcademy.Application.Commands.AttendanceCommands.UpdateAttendance
{
    public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, Result<AttendanceDto>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;
        private readonly string _operation = OperationType.Update.ToString();

        public UpdateAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            IMapper mapper)
        {
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
        }

        public async Task<Result<AttendanceDto>> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new AttendanceNotFoundException($"{request.Id}");

            _mapper.Map(request, attendance);

            cancellationToken.ThrowIfCancellationRequested();

            await _attendanceRepository.UpdateAsync(attendance, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var attendanceDto = _mapper.Map<AttendanceDto>(attendance)
                ?? throw new AutoMapperMappingException("Error occurred while mapping Attendance to DTO.");

            return Result<AttendanceDto>.Success(attendanceDto, _operation);
        }
    }
}

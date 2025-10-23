using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.AttendanceExceptions;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetById
{
    public class GetAttendanceQueryHandler : IRequestHandler<GetAttendanceByIdQuery, Result<AttendanceDto>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;
        private readonly string _operation = OperationType.Get.ToString();

        public GetAttendanceQueryHandler(
            IAttendanceRepository attendanceRepository,
            IMapper mapper)
        {
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
        }

        public async Task<Result<AttendanceDto>> Handle(GetAttendanceByIdQuery request, CancellationToken cancellationToken)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new AttendanceNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            var attendanceDto = _mapper.Map<AttendanceDto>(attendance)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<AttendanceDto>.Success(attendanceDto, _operation);
        }
    }
}

using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetAll
{
    public class GetAttendancesQueryHandler : IRequestHandler<GetAttendancesQuery, Result<List<AttendanceDto>>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;
        private readonly string _operation = OperationType.GetAll.ToString();

        public GetAttendancesQueryHandler(
            IAttendanceRepository attendanceRepository,
            IMapper mapper)
        {
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<AttendanceDto>>> Handle(GetAttendancesQuery request, CancellationToken cancellationToken)
        {
            var attendanceEntities = await _attendanceRepository.GetAllAsync(cancellationToken)
                ?? [];

            var attendances = _mapper.Map<List<AttendanceDto>>(attendanceEntities)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<List<AttendanceDto>>.Success(attendances, _operation);
        }
    }
}

using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetAll
{
    public class GetAttendancesQueryHandler : IRequestHandler<GetAttendancesQuery, Result<PagedData<AttendanceDto>>>
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

        public async Task<Result<PagedData<AttendanceDto>>> Handle(GetAttendancesQuery request, CancellationToken cancellationToken)
        {
            var attendancesDto = await _attendanceRepository.GetAllAsync(request.Page, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<PagedData<AttendanceDto>>.Success(attendancesDto, _operation);
        }
    }
}

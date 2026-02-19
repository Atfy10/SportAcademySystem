using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetGlobalAttendanceRate
{
    public class GetGlobalAttendanceRateQueryHandler : IRequestHandler<GetGlobalAttendanceRateQuery, Result<int>>
    {
        IAttendanceRepository _attendanceRepository;

        public GetGlobalAttendanceRateQueryHandler(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<Result<int>> Handle(GetGlobalAttendanceRateQuery request, CancellationToken ct)
        {
            var attendanceRate = await _attendanceRepository.GetGlobalAttendanceRate(ct);
            
            return Result<int>.Success(attendanceRate, nameof(GetGlobalAttendanceRateQuery));
        }
    }
}

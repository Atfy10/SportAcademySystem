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
            int attendanceRate = 0;
            try
            {
                if (request.Month.HasValue)
                    attendanceRate = await _attendanceRepository.GetMonthlyAttendanceRate(request.Month.Value, request.Year, ct);
                else
                    attendanceRate = await _attendanceRepository.GetGlobalAttendanceRate(ct);
            }
            catch (DivideByZeroException)
            {
                return Result<int>.Failure(nameof(GetGlobalAttendanceRateQuery), "No attendance records found.");
            }

            return Result<int>.Success(attendanceRate, nameof(GetGlobalAttendanceRateQuery));
        }
    }
}

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
            // Nothing marked yet is a 0% rate, not an error (this used to divide by zero and
            // come back as a failed request).
            var attendanceRate = request.Month.HasValue
                ? await _attendanceRepository.GetMonthlyAttendanceRate(request.Month.Value, request.Year, ct)
                : await _attendanceRepository.GetGlobalAttendanceRate(ct);

            return Result<int>.Success(attendanceRate, nameof(GetGlobalAttendanceRateQuery));
        }
    }
}

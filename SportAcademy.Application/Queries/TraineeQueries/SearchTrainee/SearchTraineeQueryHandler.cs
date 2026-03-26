using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.EmployeeQueries.SearchEmployeess;
using System.Diagnostics;

namespace SportAcademy.Application.Queries.TraineeQueries.SearchTrainee
{
    public class SearchTraineeQueryHandler
        : IRequestHandler<SearchTraineeQuery, Result<PagedData<TraineeCardDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IAttendanceRepository _attendanceRepository;

        public SearchTraineeQueryHandler(
            ITraineeRepository traineeRepository,
            IAttendanceRepository attendanceRepository)
        {
            _traineeRepository = traineeRepository;
            _attendanceRepository = attendanceRepository;
        }

        public async Task<Result<PagedData<TraineeCardDto>>> Handle(
            SearchTraineeQuery request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var trainees = await _traineeRepository.SearchAsync(
                request.Term,
                request.Page,
                cancellationToken
            );

            foreach (var trainee in trainees.Items)
            {
                (int totalSessions, int attendendedSessions) = await _attendanceRepository.GetAttendanceSummaryAsync(trainee.Id, null, null, cancellationToken);
                trainee.AttendanceRate = totalSessions == 0
                    ? 0
                    : Math.Round((double)(attendendedSessions / totalSessions * 100), 2);
            }

            sw.Stop();

            return Result<PagedData<TraineeCardDto>>.Success(trainees, nameof(SearchTraineeQuery));
        }
    }
}

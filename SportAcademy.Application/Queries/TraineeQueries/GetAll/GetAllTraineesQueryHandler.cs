using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAll
{
    public class GetAllTraineesQueryHandler : IRequestHandler<GetAllTraineesQuery, Result<PagedData<TraineeCardDto>>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllTraineesQueryHandler(
            IAttendanceRepository attendanceRepository,
            ITraineeRepository traineeRepository
        )
        {
            _traineeRepository = traineeRepository;
            _attendanceRepository = attendanceRepository;
        }

        public async Task<Result<PagedData<TraineeCardDto>>> Handle(GetAllTraineesQuery request, CancellationToken cancellationToken)
        {
            var hasFilters = !string.IsNullOrEmpty(request.Sport) || !string.IsNullOrEmpty(request.Status);
            var hasSort = !string.IsNullOrEmpty(request.SortBy);

            var traineesDto = (hasFilters || hasSort)
                ? await _traineeRepository.SearchAsync("", request.Page, request.Sport, request.Status, request.SortBy, request.SortDir, cancellationToken)
                : await _traineeRepository.GetAllPaginatedAsync<TraineeCardDto>(request.Page, cancellationToken);

            foreach (var trainee in traineesDto.Items)
            {
                (int totalSessions, int attendendedSessions) = await _attendanceRepository.GetAttendanceSummaryAsync(trainee.Id, null, null, cancellationToken);
                trainee.AttendanceRate = totalSessions == 0
                    ? 0
                    : Math.Round((double)(attendendedSessions / totalSessions * 100), 2);
            }

            return Result<PagedData<TraineeCardDto>>.Success(traineesDto, _operationType);
        }
    }
}

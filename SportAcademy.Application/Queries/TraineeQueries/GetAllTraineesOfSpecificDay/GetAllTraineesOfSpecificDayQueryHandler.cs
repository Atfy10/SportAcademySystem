using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAllTraineesOfSpecificDay
{
    public class GetAllTraineesOfSpecificDayQueryHandler : IRequestHandler<GetAllTraineesOfSpecificDayQuery, Result<PagedData<TraineeOfSpecificDayDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;

        public GetAllTraineesOfSpecificDayQueryHandler(
            ITraineeRepository traineeRepository,
            IAttendanceRepository attendanceRepository,
            IMapper mapper)
        {
            _mapper = mapper;
            _traineeRepository = traineeRepository;
            _attendanceRepository = attendanceRepository;
        }

        public async Task<Result<PagedData<TraineeOfSpecificDayDto>>> Handle(GetAllTraineesOfSpecificDayQuery request, CancellationToken ct)
        {
            var trainees = await _traineeRepository.GetAllTraineesOfSpecificDayAsync(request.Date, request.Page, ct);

            // The repository leaves AttendanceRate at 0 - fill in the real one for the page.
            var rates = await _attendanceRepository.GetAttendanceRatesAsync(trainees.Items.Select(t => t.Id).ToList(), ct);
            trainees = new PagedData<TraineeOfSpecificDayDto>
            {
                Items = trainees.Items
                    .Select(t => t with { AttendanceRate = (int)Math.Round(rates.GetValueOrDefault(t.Id)) })
                    .ToList(),
                TotalCount = trainees.TotalCount,
                Page = trainees.Page,
                PageSize = trainees.PageSize,
            };

            return Result<PagedData<TraineeOfSpecificDayDto>>.Success(trainees, nameof(GetAllTraineesOfSpecificDayQuery));
        }
    }
}

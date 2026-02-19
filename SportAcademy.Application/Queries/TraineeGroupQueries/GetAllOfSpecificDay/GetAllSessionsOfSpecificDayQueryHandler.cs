using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllOfSpecificDay
{
    public class GetAllSessionsOfSpecificDayQueryHandler : IRequestHandler<GetAllSessionsOfSpecificDayQuery, Result<PagedData<ListTraineeGroupDto>>>
    {
        private readonly IMapper _mapper;
        private readonly ITraineeGroupRepository _traineeGroupRepository;

        public GetAllSessionsOfSpecificDayQueryHandler(IMapper mapper, ITraineeGroupRepository traineeGroupRepository)
        {
            _mapper = mapper;
            _traineeGroupRepository = traineeGroupRepository;
        }

        public async Task<Result<PagedData<ListTraineeGroupDto>>> Handle(GetAllSessionsOfSpecificDayQuery request, CancellationToken ct)
        {
            var traineeGroupDto = await _traineeGroupRepository
                .GetAllOfSpecificDayAsync(request.Page, request.Day, ct);

            return Result<PagedData<ListTraineeGroupDto>>.Success(traineeGroupDto, nameof(GetAllSessionsOfSpecificDayQuery)); 
        }
    }
}

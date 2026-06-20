using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAll
{
    public class GetAllTraineeGroupsQueryHandler : IRequestHandler<GetAllTraineeGroupsQuery, Result<PagedData<TraineeGroupCardDto>>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllTraineeGroupsQueryHandler(ITraineeGroupRepository traineeGroupRepository)
        {
            _traineeGroupRepository = traineeGroupRepository;
        }

        public async Task<Result<PagedData<TraineeGroupCardDto>>> Handle(GetAllTraineeGroupsQuery request, CancellationToken cancellationToken)
        {
            var traineeGroups = await _traineeGroupRepository
                .GetAllAsCardAsync(request.Page, cancellationToken);

            return Result<PagedData<TraineeGroupCardDto>>.Success(traineeGroups, _operationType);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllForDropdown;

public class GetAllTraineeGroupsForDropdownQueryHandler(
    ITraineeGroupRepository traineeGroupRepository)
    : IRequestHandler<GetAllTraineeGroupsForDropdownQuery, Result<List<TraineeGroupDropdownDto>>>
{
    public async Task<Result<List<TraineeGroupDropdownDto>>> Handle(
        GetAllTraineeGroupsForDropdownQuery request,
        CancellationToken cancellationToken)
    {
        var items = await traineeGroupRepository.GetAllForDropdownAsync(request.SportId, cancellationToken);
        return Result<List<TraineeGroupDropdownDto>>.Success(items, OperationType.GetAll.ToString());
    }
}

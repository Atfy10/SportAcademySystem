using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetDayPatterns;

public class GetGroupDayPatternsQueryHandler(
    ITraineeGroupRepository traineeGroupRepository)
    : IRequestHandler<GetGroupDayPatternsQuery, Result<List<GroupDayPatternDto>>>
{
    public async Task<Result<List<GroupDayPatternDto>>> Handle(
        GetGroupDayPatternsQuery request,
        CancellationToken cancellationToken)
    {
        var patterns = await traineeGroupRepository.GetDayPatternsAsync(
            request.SportId, request.BranchId, request.GroupType, cancellationToken);

        return Result<List<GroupDayPatternDto>>.Success(patterns, OperationType.GetAll.ToString());
    }
}

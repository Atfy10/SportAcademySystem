using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetEligibleTraineesForGroup;

public class GetEligibleTraineesForGroupQueryHandler(ITraineeRepository traineeRepository)
    : IRequestHandler<GetEligibleTraineesForGroupQuery, Result<List<EligibleTraineeForGroupDto>>>
{
    public async Task<Result<List<EligibleTraineeForGroupDto>>> Handle(
        GetEligibleTraineesForGroupQuery request,
        CancellationToken cancellationToken)
    {
        var items = await traineeRepository.GetEligibleForGroupAsync(request.TraineeGroupId, cancellationToken);
        return Result<List<EligibleTraineeForGroupDto>>.Success(items, OperationType.GetAll.ToString());
    }
}

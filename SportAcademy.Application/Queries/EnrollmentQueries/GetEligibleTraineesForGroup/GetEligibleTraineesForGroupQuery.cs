using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetEligibleTraineesForGroup;

public record GetEligibleTraineesForGroupQuery(int TraineeGroupId)
    : IRequest<Result<List<EligibleTraineeForGroupDto>>>;

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.FamilyCommands.AddTraineeToFamily;

public record AddTraineeToFamilyCommand(int FamilyId, int TraineeId) : IRequest<Result<FamilyDetailDto>>, IRequiresFeature
{
    public string FeatureKey => "family-management";
}

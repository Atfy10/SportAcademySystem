using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.FamilyCommands.UpdateFamily;

public record UpdateFamilyCommand(
    int Id,
    string? Name,
    string? GuardianName,
    string? GuardianPhone,
    string? NameAr = null,
    string? GuardianNameAr = null
) : IRequest<Result<FamilyDto>>, IRequiresFeature
{
    public string FeatureKey => "family-management";
}

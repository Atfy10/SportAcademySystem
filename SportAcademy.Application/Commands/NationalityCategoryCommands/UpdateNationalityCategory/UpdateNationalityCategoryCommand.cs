using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.NationalityCategoryCommands.UpdateNationalityCategory
{
    public record UpdateNationalityCategoryCommand(
        int Id,
        string Code,
        string Name,
        string? NameAr = null
    ) : IRequest<Result<int>>, IRequiresFeature
    {
        public string FeatureKey => "nationality-categories";
    }
}

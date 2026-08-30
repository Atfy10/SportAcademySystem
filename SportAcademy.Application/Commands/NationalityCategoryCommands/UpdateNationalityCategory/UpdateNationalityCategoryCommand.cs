using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.NationalityCategoryCommands.UpdateNationalityCategory
{
    public record UpdateNationalityCategoryCommand(
        int Id,
        string Code,
        string Name,
        string? NameAr = null
    ) : IRequest<Result<int>>;
}

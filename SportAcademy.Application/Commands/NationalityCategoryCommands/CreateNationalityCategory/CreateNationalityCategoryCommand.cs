using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.NationalityCategoryCommands.CreateNationalityCategory
{
    public record CreateNationalityCategoryCommand(
        string Code,
        string Name,
        string? NameAr = null
    ) : IRequest<Result<int>>;
}

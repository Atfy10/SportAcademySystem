using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.DiscountCodeCommands.DeleteDiscountCode
{
    public record DeleteDiscountCodeCommand(int Id) : IRequest<Result<bool>>;
}

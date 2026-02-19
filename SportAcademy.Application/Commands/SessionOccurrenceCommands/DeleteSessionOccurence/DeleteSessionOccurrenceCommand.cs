using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.DeleteSessionOccurence
{
    public record DeleteSessionOccurrenceCommand(int Id) : IRequest<Result<bool>>;
}

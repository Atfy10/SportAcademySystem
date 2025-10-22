using MediatR;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.DeleteSessionOccurence
{
    public record DeleteSessionOccurrenceCommand(int Id) : IRequest<Result<bool>>;
}

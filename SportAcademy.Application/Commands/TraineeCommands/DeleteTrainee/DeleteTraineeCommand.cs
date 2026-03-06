using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.Trainees.DeleteTrainee
{
    public record DeleteTraineeCommand(int Id) : IRequest<Result<bool>>;
}

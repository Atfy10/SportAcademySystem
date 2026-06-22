using MediatR;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.Trainees.ImportTrainees
{
    public record ImportTraineesCommand(List<CreateTraineeCommand> Trainees)
        : IRequest<Result<ImportTraineesResult>>;
}

using MediatR;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.Trainees.ImportTrainees
{
    public record ImportTraineesCommand(List<CreateTraineeCommand> Trainees)
        : IRequest<Result<ImportTraineesResult>>, IRequiresFeature
    {
        public string FeatureKey => "trainee-management";
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.Trainees.DeleteTrainee
{
    public record DeleteTraineeCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "trainee-management";
    }
}

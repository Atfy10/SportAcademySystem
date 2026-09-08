using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.DeleteSessionOccurence
{
    public record DeleteSessionOccurrenceCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "session-management";
    }
}

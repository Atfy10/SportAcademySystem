using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType
{
    public record CreateSubscriptionTypeCommand(
        string Name,
        int DaysPerMonth,
        int NumberOfMonths,
        bool IsActive,
        bool IsOffer,
        List<int> SportIds
    ) : IRequest<Result<int>>, IRequiresFeature, IRequiresActiveSports
    {
        public string FeatureKey => "subscription-plan";

        // Explicit implementation: SportIds' declared type (List<int>) can't implicitly satisfy
        // IRequiresActiveSports.SportIds (IEnumerable<int>?) - see CreateTraineeCommand's
        // identical note.
        IEnumerable<int>? IRequiresActiveSports.SportIds => SportIds;
    }
}
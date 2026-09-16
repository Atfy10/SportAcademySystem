using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionTypeDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.UpdateSubscriptionType
{
    public record UpdateSubscriptionTypeCommand(
        int Id,
        string? Name,
        int? DaysPerMonth,
        int? NumberOfMonths,
        bool? IsActive,
        bool? IsOffer,
        List<int>? SportIds
    ) : IRequest<Result<SubscriptionTypeDto>>, IRequiresFeature, IRequiresActiveSports
    {
        public string FeatureKey => "subscription-plan";

        // Explicit implementation: SportIds' declared type (List<int>?) can't implicitly satisfy
        // IRequiresActiveSports.SportIds (IEnumerable<int>?) - see CreateTraineeCommand's
        // identical note.
        IEnumerable<int>? IRequiresActiveSports.SportIds => SportIds;
    }
}
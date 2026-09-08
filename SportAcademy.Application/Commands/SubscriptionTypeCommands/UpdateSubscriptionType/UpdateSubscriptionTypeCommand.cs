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
    ) : IRequest<Result<SubscriptionTypeDto>>, IRequiresFeature
    {
        public string FeatureKey => "subscription-plan";
    }
}
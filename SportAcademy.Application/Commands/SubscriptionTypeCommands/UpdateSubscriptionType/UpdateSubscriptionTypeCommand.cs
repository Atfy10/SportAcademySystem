using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionTypeDtos;

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
    ) : IRequest<Result<SubscriptionTypeDto>>;
}
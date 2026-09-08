using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DiscountCodeDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.DiscountCodeCommands.UpdateDiscountCode
{
    public record UpdateDiscountCodeCommand(
        int Id,
        string Code,
        string? Description,
        decimal PercentageOff,
        bool IsActive,
        DateOnly? ExpiresAt
    ) : IRequest<Result<DiscountCodeDto>>, IRequiresFeature
    {
        public string FeatureKey => "discount-offers";
    }
}

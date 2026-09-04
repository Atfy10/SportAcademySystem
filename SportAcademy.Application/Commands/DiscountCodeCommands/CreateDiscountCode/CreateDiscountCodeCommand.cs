using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DiscountCodeDtos;

namespace SportAcademy.Application.Commands.DiscountCodeCommands.CreateDiscountCode
{
    public record CreateDiscountCodeCommand(
        string Code,
        string? Description,
        decimal PercentageOff,
        bool IsActive,
        DateOnly? ExpiresAt
    ) : IRequest<Result<DiscountCodeDto>>;
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.CreatePaymentType
{
    public record CreatePaymentTypeCommand(
        string Name,
        bool IsActive,
        bool IsDefault,
        string? NameAr = null
    ) : IRequest<Result<PaymentTypeDto>>;
}

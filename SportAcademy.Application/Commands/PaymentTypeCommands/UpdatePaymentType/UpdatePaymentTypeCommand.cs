using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.UpdatePaymentType
{
    public record UpdatePaymentTypeCommand(
        int Id,
        string? Name,
        bool? IsActive,
        bool? IsDefault
    ) : IRequest<Result<PaymentTypeDto>>;
}

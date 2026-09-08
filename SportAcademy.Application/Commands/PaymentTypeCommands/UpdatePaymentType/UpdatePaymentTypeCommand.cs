using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.UpdatePaymentType
{
    public record UpdatePaymentTypeCommand(
        int Id,
        string? Name,
        bool? IsActive,
        bool? IsDefault,
        string? NameAr = null
    ) : IRequest<Result<PaymentTypeDto>>, IRequiresFeature
    {
        public string FeatureKey => "payment-processing";
    }
}

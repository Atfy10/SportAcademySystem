using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.DeletePaymentType
{
    public record DeletePaymentTypeCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "payment-processing";
    }
}

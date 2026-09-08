using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdatePaymentStatus;

public record UpdatePaymentStatusCommand(int Id, string PaymentStatus) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "enrollment-management";
}

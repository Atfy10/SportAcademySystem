using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdatePaymentStatus;

public record UpdatePaymentStatusCommand(int Id, string PaymentStatus) : IRequest<Result<bool>>;

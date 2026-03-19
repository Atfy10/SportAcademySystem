using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ActivateEnrollment;

public record ActivateEnrollmentCommand(int Id) : IRequest<Result<bool>>;

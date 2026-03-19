using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.EnrollmentCommands.SuspendEnrollment;

public record SuspendEnrollmentCommand(int Id) : IRequest<Result<bool>>;

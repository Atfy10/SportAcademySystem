using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ChangeEnrollmentGroup;

public record ChangeEnrollmentGroupCommand(int EnrollmentId, int NewTraineeGroupId) : IRequest<Result<bool>>;

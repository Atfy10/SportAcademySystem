using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.PauseTraineeGroup;

public class PauseTraineeGroupCommandHandler(
    ITraineeGroupRepository traineeGroupRepository,
    ISessionOccurrenceRepository sessionOccurrenceRepository,
    IPublisher publisher)
    : IRequestHandler<PauseTraineeGroupCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PauseTraineeGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new TraineeGroupNotFoundException(request.Id.ToString());

        group.IsActive = false;
        group.InactiveReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        await traineeGroupRepository.UpdateAsync(group, cancellationToken);

        // Sessions already generated for while the group is paused shouldn't sit there looking
        // "Scheduled" (implying attendance is still expected) - flip them to a distinct,
        // reversible status so resuming can tell these apart from a session someone permanently
        // cancelled on purpose.
        await sessionOccurrenceRepository.SetFutureSessionsStatusAsync(
            group.Id, SessionStatus.Scheduled, SessionStatus.CancelledTemporary, DateTime.Now, cancellationToken);

        await publisher.Publish(new TraineeGroupUpdatedEvent(group.Id), cancellationToken);

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

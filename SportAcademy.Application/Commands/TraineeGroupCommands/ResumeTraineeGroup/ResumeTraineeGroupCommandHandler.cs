using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.ResumeTraineeGroup;

public class ResumeTraineeGroupCommandHandler(
    ITraineeGroupRepository traineeGroupRepository,
    ISessionOccurrenceRepository sessionOccurrenceRepository,
    IPublisher publisher)
    : IRequestHandler<ResumeTraineeGroupCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ResumeTraineeGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new TraineeGroupNotFoundException(request.Id.ToString());

        group.IsActive = true;
        group.InactiveReason = null;
        await traineeGroupRepository.UpdateAsync(group, cancellationToken);

        // Only restores sessions this same pause temporarily cancelled - a session someone
        // separately, permanently Canceled is untouched.
        await sessionOccurrenceRepository.SetFutureSessionsStatusAsync(
            group.Id, SessionStatus.CancelledTemporary, SessionStatus.Scheduled, DateTime.UtcNow, cancellationToken);

        await publisher.Publish(new TraineeGroupUpdatedEvent(group.Id), cancellationToken);

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

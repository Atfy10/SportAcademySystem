using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.SwitchTraineeGroupToPrivate;

public class SwitchTraineeGroupToPrivateCommandHandler(
    ITraineeGroupRepository traineeGroupRepository,
    IEnrollmentRepository enrollmentRepository,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : IRequestHandler<SwitchTraineeGroupToPrivateCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SwitchTraineeGroupToPrivateCommand request, CancellationToken cancellationToken)
    {
        var group = await traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new TraineeGroupNotFoundException(request.Id.ToString());

        if (group.Type == TraineeGroupType.Private)
            throw new GroupAlreadyPrivateException();

        // Active and Suspended both hold a seat; only those can be kept or ended.
        var current = await enrollmentRepository.GetActiveEnrollmentsForGroupAsync(group.Id, cancellationToken);

        var keep = request.KeepTraineeIds.ToHashSet();
        if (!keep.IsSubsetOf(current.Select(e => e.TraineeId)))
            throw new KeptTraineeNotInGroupException();

        // The trainees who aren't carrying on leave the group now. Their subscription is untouched
        // - it simply stops being tied to an open enrollment, so they can be enrolled into another
        // group and use up what's left of it there.
        var leaving = current.Where(e => !keep.Contains(e.TraineeId)).ToList();
        var now = DateTime.UtcNow;
        foreach (var enrollment in leaving)
        {
            enrollment.Status = EnrollmentStatus.Ended;
            enrollment.EndDate = now;
        }

        group.Type = TraineeGroupType.Private;
        group.MaximumCapacity = request.MaximumCapacity;

        cancellationToken.ThrowIfCancellationRequested();

        // One save: the group flips to private and the leavers are ended together or not at all.
        await traineeGroupRepository.UpdateAsyncWithoutSave(group, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new TraineeGroupUpdatedEvent(group.Id), cancellationToken);

        return Result<int>.Success(leaving.Count, OperationType.Update.ToString());
    }
}

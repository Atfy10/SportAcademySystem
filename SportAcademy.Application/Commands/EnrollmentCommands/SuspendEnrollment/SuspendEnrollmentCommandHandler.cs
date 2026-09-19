using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;

namespace SportAcademy.Application.Commands.EnrollmentCommands.SuspendEnrollment;

public class SuspendEnrollmentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IUserContextService userContext,
    IUserRepository userRepository,
    IPublisher publisher)
    : IRequestHandler<SuspendEnrollmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        SuspendEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await ((IBaseRepository<Enrollment, int>)enrollmentRepository)
            .GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Domain.Exceptions.BaseExceptions.IdNotFoundException("Enrollment", request.Id.ToString());

        // Suspend pauses a live enrollment - it isn't a way to close one that's already ended
        // or already run past its expiry date. ExpiryDate/SessionRemaining are deliberately left
        // untouched below - only Status changes, so nothing here needs "undoing" on reactivate.
        if (enrollment.Status == EnrollmentStatus.Ended)
            throw new EnrollmentAlreadyExpiredException(enrollment.Id);
        if (enrollment.ExpiryDate < DateTime.UtcNow)
            throw new EnrollmentAlreadyExpiredException(enrollment.Id);

        enrollment.Status = EnrollmentStatus.Suspended;
        await enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        var actorName = userContext.UserId is { } userId
            ? await userRepository.GetDisplayNameAsync(userId, cancellationToken)
            : "System";
        await publisher.Publish(new EnrollmentLifecycleEvent(enrollment.Id, "Suspended", actorName), cancellationToken);

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

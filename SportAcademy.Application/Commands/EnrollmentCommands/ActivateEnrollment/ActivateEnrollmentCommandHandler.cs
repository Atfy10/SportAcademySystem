using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ActivateEnrollment;

public class ActivateEnrollmentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IUserContextService userContext,
    IUserRepository userRepository,
    IPublisher publisher)
    : IRequestHandler<ActivateEnrollmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ActivateEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await ((IBaseRepository<Domain.Entities.Enrollment, int>)enrollmentRepository)
            .GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Domain.Exceptions.BaseExceptions.IdNotFoundException("Enrollment", request.Id.ToString());

        enrollment.IsActive = true;
        await enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        var actorName = userContext.UserId is { } userId
            ? await userRepository.GetDisplayNameAsync(userId, cancellationToken)
            : "System";
        await publisher.Publish(new EnrollmentLifecycleEvent(enrollment.Id, "Activated", actorName), cancellationToken);

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

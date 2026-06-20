using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.EnrollmentCommands.SuspendEnrollment;

public class SuspendEnrollmentCommandHandler(
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<SuspendEnrollmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        SuspendEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await ((IBaseRepository<Enrollment, int>)enrollmentRepository)
            .GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Domain.Exceptions.BaseExceptions.IdNotFoundException("Enrollment", request.Id.ToString());

        enrollment.Suspend();
        await enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

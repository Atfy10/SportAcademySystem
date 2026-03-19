using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ActivateEnrollment;

public class ActivateEnrollmentCommandHandler(
    IEnrollmentRepository enrollmentRepository)
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

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

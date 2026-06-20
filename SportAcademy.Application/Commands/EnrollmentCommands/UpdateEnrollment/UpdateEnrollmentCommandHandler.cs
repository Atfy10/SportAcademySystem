using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment
{
    public class UpdateEnrollmentCommandHandler : IRequestHandler<UpdateEnrollmentCommand, Result<EnrollmentDto>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateEnrollmentCommandHandler(
            IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<Result<EnrollmentDto>> Handle(UpdateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepository
                .GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EnrollmentNotFoundException($"{request.Id}");

            enrollment.ExtendExpiry(request.ExpiryDate);
            enrollment.AdjustSessionRemaining(request.SessionRemaining);
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    enrollment.Activate();
                else
                    enrollment.Suspend();
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var enrollmentDto = enrollment.ToDto();

            return Result<EnrollmentDto>.Success(enrollmentDto, _operationType);
        }
    }
}

using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.EnrollmentCommands.DeleteEnrollment
{
    public class DeleteEnrollmentCommandHandler : IRequestHandler<DeleteEnrollmentCommand, Result<bool>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteEnrollmentCommandHandler(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<Result<bool>> Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EnrollmentNotFoundException($"{request.Id}");

            cancellationToken.ThrowIfCancellationRequested();

            await _enrollmentRepository.DeleteAsync(enrollment, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operationType);
        }
    }
}

using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment
{
    public class UpdateEnrollmentCommandHandler : IRequestHandler<UpdateEnrollmentCommand, Result<EnrollmentDto>>
    {
        private readonly IMapper _mapper;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateEnrollmentCommandHandler(
            IMapper mapper,
            IEnrollmentRepository enrollmentRepository)
        {
            _mapper = mapper;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<Result<EnrollmentDto>> Handle(UpdateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EnrollmentNotFoundException($"{request.Id}");

            _mapper.Map(request, enrollment);

            cancellationToken.ThrowIfCancellationRequested();

            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var enrollmentDto = _mapper.Map<EnrollmentDto>(enrollment)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<EnrollmentDto>.Success(enrollmentDto, _operationType);
        }
    }
}

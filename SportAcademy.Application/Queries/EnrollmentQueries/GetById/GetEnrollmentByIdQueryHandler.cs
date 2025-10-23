using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetById
{
    public class GetEnrollmentByIdQueryHandler : IRequestHandler<GetEnrollmentByIdQuery, Result<EnrollmentDto>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetEnrollmentByIdQueryHandler(
            IEnrollmentRepository enrollmentRepository,
            IMapper mapper)
        {
            _enrollmentRepository = enrollmentRepository;
            _mapper = mapper;
        }

        public async Task<Result<EnrollmentDto>> Handle(GetEnrollmentByIdQuery request, CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EnrollmentNotFoundException($"{request.Id}");

            var enrollmentDto = _mapper.Map<EnrollmentDto>(enrollment)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<EnrollmentDto>.Success(enrollmentDto, _operationType);
        }
    }
}

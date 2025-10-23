using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetAll
{
    public class GetAllEnrollmentsQueryHandler : IRequestHandler<GetAllEnrollmentsQuery, Result<List<EnrollmentDto>>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllEnrollmentsQueryHandler(
            IEnrollmentRepository enrollmentRepository,
            IMapper mapper)
        {
            _enrollmentRepository = enrollmentRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<EnrollmentDto>>> Handle(GetAllEnrollmentsQuery request, CancellationToken cancellationToken)
        {
            var enrollments = await _enrollmentRepository.GetAllAsync(cancellationToken) 
                ?? [];

            var enrollmentsDto = _mapper.Map<List<EnrollmentDto>>(enrollments) 
                ?? [];

            return Result<List<EnrollmentDto>>.Success(enrollmentsDto, _operationType);
        }
    }
}

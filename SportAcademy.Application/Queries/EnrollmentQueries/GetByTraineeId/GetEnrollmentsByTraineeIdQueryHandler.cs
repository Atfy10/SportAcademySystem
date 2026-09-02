using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetByTraineeId
{
    public class GetEnrollmentsByTraineeIdQueryHandler : IRequestHandler<GetEnrollmentsByTraineeIdQuery, Result<List<EnrollmentDetailDto>>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetEnrollmentsByTraineeIdQueryHandler(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<Result<List<EnrollmentDetailDto>>> Handle(GetEnrollmentsByTraineeIdQuery request, CancellationToken cancellationToken)
        {
            var enrollments = await _enrollmentRepository.GetAllDetailsByTraineeIdAsync(request.TraineeId, cancellationToken);
            return Result<List<EnrollmentDetailDto>>.Success(enrollments, _operationType);
        }
    }
}

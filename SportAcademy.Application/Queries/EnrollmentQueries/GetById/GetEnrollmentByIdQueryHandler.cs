using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetById
{
    public class GetEnrollmentByIdQueryHandler(
        IEnrollmentRepository enrollmentRepository)
        : IRequestHandler<GetEnrollmentByIdQuery, Result<EnrollmentDetailDto>>
    {
        public async Task<Result<EnrollmentDetailDto>> Handle(
            GetEnrollmentByIdQuery request,
            CancellationToken cancellationToken)
        {
            var dto = await enrollmentRepository.GetDetailByIdAsync(request.Id, cancellationToken);
            return Result<EnrollmentDetailDto>.Success(dto!, OperationType.Get.ToString());
        }
    }
}

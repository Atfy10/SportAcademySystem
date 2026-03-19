using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetById
{
    public record GetEnrollmentByIdQuery(int Id) : IRequest<Result<EnrollmentDetailDto>>;
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetByTraineeId
{
    public record GetEnrollmentsByTraineeIdQuery(int TraineeId) : IRequest<Result<List<EnrollmentDetailDto>>>;
}

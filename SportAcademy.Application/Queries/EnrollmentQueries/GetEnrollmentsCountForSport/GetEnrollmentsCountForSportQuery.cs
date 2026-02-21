using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSport;

public record GetEnrollmentsCountForSportQuery(
    int SportId,
    DateTime? From,
    DateTime? To
) : IRequest<Result<int>>;

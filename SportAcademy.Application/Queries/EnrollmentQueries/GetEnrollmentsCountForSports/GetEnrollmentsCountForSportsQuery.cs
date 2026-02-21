using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSports;

public record GetEnrollmentsCountForSportsQuery(
    DateTime? From,
    DateTime? To
) : IRequest<Result<int>>;

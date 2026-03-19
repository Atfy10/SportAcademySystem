using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EnrollmentQueries.CountAll;

public record CountAllEnrollmentsQuery : IRequest<Result<int>>;

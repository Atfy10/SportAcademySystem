using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EnrollmentQueries.CountActive;

public record CountActiveEnrollmentsQuery : IRequest<Result<int>>;

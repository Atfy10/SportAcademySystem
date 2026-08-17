using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;

namespace SportAcademy.Application.Queries.EnrollmentQueries.SearchEnrollments;

public record SearchEnrollmentsQuery(
    string Term,
    PageRequest Page,
    string? Status = null,
    string? PaymentStatus = null
) : IRequest<Result<PagedData<EnrollmentCardDto>>>;

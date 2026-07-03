using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetTenants;

public record GetTenantsQuery(
    int? Page = null,
    int? PageSize = null,
    string? Status = null,
    string? Search = null
) : IRequest<Result<PagedData<TenantListResponse>>>;

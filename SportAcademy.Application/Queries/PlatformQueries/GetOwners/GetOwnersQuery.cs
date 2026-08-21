using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetOwners;

public record GetOwnersQuery(
    int? Page = null,
    int? PageSize = null,
    string? Search = null
) : IRequest<Result<PagedData<OwnerListItemDto>>>;

using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetTenants;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, Result<PagedData<TenantListResponse>>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<PagedData<TenantListResponse>>> Handle(GetTenantsQuery request, CancellationToken ct)
    {
        var page = PageRequest.Create(request.Page, request.PageSize);
        var (items, totalCount) = await _tenantRepository.GetPagedAsync(
            page.Skip, page.PageSize, request.Status, request.Search, ct);

        var data = new PagedData<TenantListResponse>
        {
            Items = items.Select(t => t.ToListResponse()).ToList(),
            TotalCount = totalCount,
            Page = page.Page,
            PageSize = page.PageSize
        };

        return Result<PagedData<TenantListResponse>>.Success(data, _operation);
    }
}

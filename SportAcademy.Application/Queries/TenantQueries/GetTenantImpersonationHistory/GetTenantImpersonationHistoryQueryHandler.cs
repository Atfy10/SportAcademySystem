using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantImpersonationHistory;

public class GetTenantImpersonationHistoryQueryHandler
    : IRequestHandler<GetTenantImpersonationHistoryQuery, Result<PagedData<TenantImpersonationGrantDto>>>
{
    private readonly IImpersonationGrantRepository _grantRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public GetTenantImpersonationHistoryQueryHandler(
        IImpersonationGrantRepository grantRepository, IUserContextService userContext)
    {
        _grantRepository = grantRepository;
        _userContext = userContext;
    }

    public async Task<Result<PagedData<TenantImpersonationGrantDto>>> Handle(
        GetTenantImpersonationHistoryQuery request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result<PagedData<TenantImpersonationGrantDto>>.Failure(_operation, "Tenant ID is not available.", 400);

        var pageRequest = PageRequest.Create(request.Page, request.PageSize);
        var data = await _grantRepository.GetPagedForTenantAsync(tenantId.Value, pageRequest, ct);

        return Result<PagedData<TenantImpersonationGrantDto>>.Success(data, _operation);
    }
}

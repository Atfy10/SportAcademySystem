using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantImpersonationHistory;

// Tenant-scoped (unlike everything else about impersonation, which is platform-scoped) -
// this is what lets a tenant's own Owner see who from the platform accessed their account and
// when. Answers the "does the tenant get told?" question the plan raised: yes, this makes the
// record visible to them, not just to the platform operator.
public record GetTenantImpersonationHistoryQuery(int? Page, int? PageSize)
    : IRequest<Result<PagedData<TenantImpersonationGrantDto>>>;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers.Platform;

// Read-only controller (single GetAuditLog action), so PlatformSupport is allowed here unlike
// the other platform controllers whose class-level role check is SuperAdmin-only.
[Authorize(Roles = "SuperAdmin,PlatformSupport")]
// per-user, not per-tenant: see TenantsController for why (F-11).
[EnableRateLimiting("per-user")]
[Route("api/platform/audit")]
[ApiController]
[Authorize(Policy = "Permission:platform.audit.read")]
public class AuditController : ControllerBase
{
    private readonly ITenantAuditRepository _auditRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public AuditController(ITenantAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLog(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? type,
        [FromQuery] AuditOutcome? outcome,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var pageRequest = PageRequest.Create(page, pageSize);
        var data = await _auditRepository.GetPagedAsync(tenantId, type, outcome, from, to, pageRequest, ct);
        var result = Result<PagedData<Application.DTOs.PlatformDtos.TenantAuditEventDto>>.Success(data, _operation);
        return StatusCode(result.StatusCode, result);
    }
}

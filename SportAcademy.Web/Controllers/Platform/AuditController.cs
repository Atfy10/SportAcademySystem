using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers.Platform;

[Authorize(Roles = "SuperAdmin")]
[EnableRateLimiting("per-tenant")]
[Route("api/platform/audit")]
[ApiController]
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
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var pageRequest = PageRequest.Create(page, pageSize);
        var data = await _auditRepository.GetPagedAsync(tenantId, type, from, to, pageRequest, ct);
        return Ok(Result<PagedData<Application.DTOs.PlatformDtos.TenantAuditEventDto>>.Success(data, _operation));
    }
}

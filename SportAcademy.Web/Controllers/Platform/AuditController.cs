using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers.Platform;

[Authorize(Roles = "SuperAdmin")]
[EnableRateLimiting("per-tenant")]
[Route("api/platform/audit")]
[ApiController]
public class AuditController : ControllerBase
{
    private readonly string _operation = OperationType.GetAll.ToString();

    [HttpGet]
    public IActionResult GetAuditLog(
        [FromQuery] string? tenantId,
        [FromQuery] string? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var pageRequest = PageRequest.Create(page, pageSize);
        var result = Result<PagedData<object>>.Success(
            new PagedData<object>
            {
                Items = [],
                TotalCount = 0,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            },
            _operation);

        return Ok(result);
    }
}

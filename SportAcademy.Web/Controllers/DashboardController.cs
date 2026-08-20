using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.TenantCommands.ActivateTenant;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.DashboardQueries.GetDashboardSummary;

namespace SportAcademy.Web.Controllers
{
[Authorize]
[EnableRateLimiting("per-user")]
[Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserContextService _userContext;

        public DashboardController(IMediator mediator, IUserContextService userContext)
        {
            _mediator = mediator;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            var tenantId = _userContext.TenantId;
            if (tenantId.HasValue)
            {
                await _mediator.Send(new ActivateTenantCommand(tenantId.Value), ct);
            }

            var result = await _mediator.Send(new GetDashboardSummaryQuery(), ct);
            return Ok(result);
        }
    }
}

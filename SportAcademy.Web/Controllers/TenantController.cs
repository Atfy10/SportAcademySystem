using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.TenantCommands.BulkUpdateTenantFeatures;
using SportAcademy.Application.Commands.TenantCommands.ImportTenantSettings;
using SportAcademy.Application.Commands.TenantCommands.UpdateTenantFeature;
using SportAcademy.Application.Commands.TenantCommands.UpdateTenantSettings;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Queries.TenantQueries.ExportTenantSettings;
using SportAcademy.Application.Queries.TenantQueries.GetTenantFeatures;
using SportAcademy.Application.Queries.TenantQueries.GetTenantProfile;
using SportAcademy.Application.Queries.TenantQueries.GetTenantSettings;
using SportAcademy.Application.Queries.TenantQueries.GetTenantSettingsOptions;
using SportAcademy.Application.Queries.TenantQueries.GetCurrentTenantQuery;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/tenant")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TenantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentTenant(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCurrentTenantQuery(), ct);
            return Ok(result);
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTenantSettingsQuery(), ct);
            return Ok(result);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTenantProfileQuery(), ct);
            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateTenantProfileCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateTenantSettingsCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpGet("features")]
        public async Task<IActionResult> GetFeatures(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTenantFeaturesQuery(), ct);
            return Ok(result);
        }

        [HttpPut("features/{featureId}")]
        public async Task<IActionResult> UpdateFeature(
            [FromRoute] Guid featureId,
            [FromBody] UpdateTenantFeatureRequest request,
            CancellationToken ct)
        {
            if (request.FeatureId != featureId) return BadRequest();
            var result = await _mediator.Send(new UpdateTenantFeatureCommand(featureId, request.IsEnabled), ct);
            return Ok(result);
        }

        [HttpPost("features/bulk")]
        public async Task<IActionResult> BulkUpdateFeatures(
            [FromBody] BulkUpdateTenantFeaturesRequest request,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new BulkUpdateTenantFeaturesCommand(request.FeatureStates), ct);
            return Ok(result);
        }

        [HttpGet("settings/export")]
        public async Task<IActionResult> ExportSettings(CancellationToken ct)
        {
            var result = await _mediator.Send(new ExportTenantSettingsQuery(), ct);
            return Ok(result);
        }

        [HttpPost("settings/import")]
        public async Task<IActionResult> ImportSettings(
            [FromBody] ImportTenantSettingsCommand command,
            CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpGet("settings/options")]
        public async Task<IActionResult> GetSettingsOptions(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTenantSettingsOptionsQuery(), ct);
            return Ok(result);
        }
    }

    public record UpdateTenantFeatureRequest(Guid FeatureId, bool IsEnabled);
    public record BulkUpdateTenantFeaturesRequest(Dictionary<Guid, bool> FeatureStates);
}

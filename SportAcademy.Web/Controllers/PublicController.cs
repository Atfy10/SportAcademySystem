using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.MarketingCommands.CreateLead;
using SportAcademy.Application.Queries.PublicQueries.GetPublicPlans;

namespace SportAcademy.Web.Controllers;

// Everything here is anonymous, public-marketing-site-facing surface: no tenant context, no
// user account. Kept as its own controller (not folded into Platform/TenantsController or
// OnboardingController) so "what can an anonymous internet visitor reach" stays one file to
// audit, not scattered [AllowAnonymous] actions across controllers that are otherwise
// [Authorize]-gated by default.
[ApiController]
[AllowAnonymous]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Cache-friendly: the marketing site's static build fetches this at build time and a
    // small client-side island re-fetches it on page load, so a SuperAdmin price edit in the
    // Platform console shows up on the public site within one HTTP cache TTL, no rebuild.
    [HttpGet("plans")]
    [EnableRateLimiting("public")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPublicPlansQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("leads")]
    [EnableRateLimiting("public-lead")]
    public async Task<IActionResult> CreateLead([FromBody] CreateLeadRequest request, CancellationToken ct)
    {
        // SHA-256(ip + nothing else stored) - just enough to correlate repeated submissions
        // from one client for abuse review; never the raw IP, and never logged elsewhere.
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ipHash = remoteIp is null ? null : Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(remoteIp)));

        var command = new CreateLeadCommand(
            request.FullName,
            request.AcademyName,
            request.Email,
            request.PhoneNumber,
            request.City,
            request.BranchCount,
            request.TraineeCountBand,
            request.Message,
            string.IsNullOrWhiteSpace(request.Locale) ? "en" : request.Locale,
            request.SourcePage,
            request.UtmSource,
            request.UtmMedium,
            request.UtmCampaign,
            Request.Headers.Referer.FirstOrDefault(),
            ipHash,
            request.Website, // honeypot field - a real visitor never sees or fills this
            request.FormRenderedAtUtc);

        var result = await _mediator.Send(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}

// A flat request DTO rather than binding CreateLeadCommand directly - IpHash and Referrer are
// server-derived (see above), never trusted from the client body.
public record CreateLeadRequest(
    string FullName,
    string AcademyName,
    string Email,
    string PhoneNumber,
    string? City,
    int? BranchCount,
    int? TraineeCountBand,
    string? Message,
    string? Locale,
    string? SourcePage,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? Website,
    DateTime? FormRenderedAtUtc);

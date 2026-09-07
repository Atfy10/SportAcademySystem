using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.FileCommands.UploadImage;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers;

// One shared upload-then-reference endpoint for every image this app stores (a user's own
// avatar, an Employee/Trainee/Coach's photo, a tenant's logo) rather than teaching every
// create/update command to accept multipart form data itself - the frontend uploads the file
// here first, gets back a URL, then submits the normal JSON command with that URL in whichever
// field it belongs to (Profile.ProfileImageUrl, Person.ImageUrl, TenantProfile.LogoUrl).
[Authorize]
[EnableRateLimiting("per-user")]
[Route("api/uploads")]
[ApiController]
public class UploadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UploadsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(
        IFormFile file, [FromQuery] ImageUploadCategory category, CancellationToken ct)
    {
        if (file is null)
            return BadRequest("File is required.");

        await using var stream = file.OpenReadStream();
        var command = new UploadImageCommand(stream, file.FileName, file.ContentType, file.Length, category);

        var result = await _mediator.Send(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}

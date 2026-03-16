using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.VideoAnalysisCommands.AnalyzeVideo;
using SportAcademy.Application.Queries.VideoAnalysisQueries.GetAnalysisById;
using SportAcademy.Application.Queries.VideoAnalysisQueries.GetUserAnalyses;

namespace SportAcademy.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VideoAnalysisController : ControllerBase
{
    private readonly IMediator _mediator;

    public VideoAnalysisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze(AnalyzeVideoCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAnalysisByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAnalyses(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserAnalysesQuery(), cancellationToken);
        return Ok(result);
    }
}

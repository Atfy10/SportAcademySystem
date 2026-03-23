using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NationalityDtos;
using SportAcademy.Application.Queries.NationalityQueries.GetAll;

namespace SportAcademy.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NationalitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NationalitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<NationalityDto>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllQuery(), cancellationToken);
        return Ok(result);
    }
}

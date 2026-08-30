using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.NationalityCategoryCommands.CreateNationalityCategory;
using SportAcademy.Application.Commands.NationalityCategoryCommands.UpdateNationalityCategory;
using SportAcademy.Application.Queries.NationalityCategoryQueries.GetAll;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
[Route("api/[controller]")]
    [ApiController]
    public class NationalityCategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NationalityCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllQuery(), cancellationToken);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:nationalitycategory.manage")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateNationalityCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:nationalitycategory.manage")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateNationalityCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command with { Id = id }, cancellationToken);
            return Ok(result);
        }

    }
}

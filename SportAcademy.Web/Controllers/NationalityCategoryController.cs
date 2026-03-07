using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Queries.NationalityCategoryQueries.GetAll;

namespace SportAcademy.Web.Controllers
{
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

    }
}

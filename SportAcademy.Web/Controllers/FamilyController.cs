using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.FamilyQueries.GetAllFamilies;
using SportAcademy.Application.Queries.FamilyQueries.SearchFamily;

namespace SportAcademy.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FamilyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetFamilies(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllFamiliesQuery(PageRequest.Create(page, pageSize)),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetFamilies(
            [FromQuery] string searchTerm,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new SearchFamilyQuery(searchTerm),
                cancellationToken);
            return Ok(result);
        }
    }
}

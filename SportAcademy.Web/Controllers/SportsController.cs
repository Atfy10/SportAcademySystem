using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SportCommands.CreateSport;
using SportAcademy.Application.Commands.SportCommands.UpdateSport;
using SportAcademy.Application.Queries.SportQueries.GetAll;
using SportAcademy.Application.Queries.SportQueries.GetAvailableSportsForBranch;

namespace SportAcademy.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateSportCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
		}

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateSportCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
		}

		[HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllSportsQuery());
            return Ok(result);
        }

        [HttpGet("get-available-for-branch/{branchId}")]
        public async Task<IActionResult> GetAvailableForBranch(int branchId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAvailableSportsForBranchQuery(branchId), cancellationToken);
            return Ok(result);
        }
    }
}

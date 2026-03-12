using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.BranchCommands.AddSportToBranch;
using SportAcademy.Application.Commands.BranchCommands.CreateBranch;
using SportAcademy.Application.Commands.BranchCommands.DeleteBranch;
using SportAcademy.Application.Commands.BranchCommands.UpdateBranch;
using SportAcademy.Application.Queries.BranchQueries;
using SportAcademy.Application.Queries.BranchQueries.GetAll;
using SportAcademy.Application.Queries.BranchQueries.GetBranchesCount;
using SportAcademy.Application.Queries.BranchQueries.GetById;


namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
	[ApiController]
	public class BranchController : ControllerBase
	{
		private readonly IMediator _mediator;

		public BranchController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateBranchCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllAttendancesQuery());
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetBranchByIdQuery(id));
			return Ok(result);
		}

		[HttpPut]
		public async Task<IActionResult> Update([FromBody] UpdateBranchCommand command,
			CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _mediator.Send(new DeleteBranchCommand(id));
			return Ok(result);
		}

        [HttpPost("branch-sports")]
        public async Task<IActionResult> AddSportToBranch([FromBody] AddSportToBranchCommand command,
			CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

		[HttpGet("count")]
		public async Task<IActionResult> GetBranchesCount(CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(new GetBranchesCountQuery(), cancellationToken);
			return Ok(result);
        }

        [HttpGet("{id}/capacity")]
        public async Task<IActionResult> GetBranchCapacity(int id,CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetBranchTotalCapacityQuery(id),ct);

            return Ok(result);
        }
    }
}

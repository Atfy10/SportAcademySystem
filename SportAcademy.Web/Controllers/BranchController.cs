using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.BranchCommands.CreateBranch;
using SportAcademy.Application.Queries.BranchQueries.GetAll;
using SportAcademy.Application.Queries.BranchQueries;
using SportAcademy.Application.Queries.BranchQueries.GetById;


namespace SportAcademy.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BranchController : ControllerBase
	{
		private readonly IMediator _mediator;

		public BranchController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("create")]
		public async Task<IActionResult> Create([FromBody] CreateBranchCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpGet("get-all")]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllBranchsQuery());
			return Ok(result);
		}

		[HttpGet("get-by-id/{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetBranchByIdQuery(id));
			return Ok(result);
		}

	}
}

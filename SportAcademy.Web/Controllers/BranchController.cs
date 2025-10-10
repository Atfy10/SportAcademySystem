using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.BranchCommands.CreateBranch;

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
	}
}

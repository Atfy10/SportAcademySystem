using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee;
using SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee;

namespace SportAcademy.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SportTrainee : ControllerBase
	{
		private readonly IMediator _mediator;
		public SportTrainee(IMediator mediator)
		{
			_mediator = mediator;
		}
		[HttpPost("create")]
		public async Task<IActionResult> CreateSportTrainee(CreateSportTraineeCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpPut("update")]
		public async Task<IActionResult> UpdateSportTrainee(UpdateSportTraineeCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}
	}
}

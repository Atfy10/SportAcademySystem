using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee;
using SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee;
using SportAcademy.Application.Commands.SportTraineeCommands.DeleteSportTrainee;
using SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee;
using SportAcademy.Application.Queries.SportTraineeQueries.GetAll;
using SportAcademy.Application.Queries.SportTraineeQueries.GetById;

namespace SportAcademy.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SportTraineeController : ControllerBase
	{
		private readonly IMediator _mediator;
		public SportTraineeController(IMediator mediator)
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

		[HttpDelete("delete")]
		public async Task<IActionResult> Delete(int sportId, int traineeId, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(new DeleteSportTraineeCommand(sportId, traineeId), cancellationToken);
			return Ok(result);

		}

		[HttpGet("get-all")]
		public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(new GetAllSportTraineesQuery(), cancellationToken);
			return Ok(result);
		}

		//get by id 
		[HttpGet("get/{sportId}/{traineeId}")]
		public async Task<IActionResult> GetById(int sportId, int traineeId, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(new GetSportTraineeByKeyQuery(sportId, traineeId), cancellationToken);
			return Ok(result);
		}
	}
}

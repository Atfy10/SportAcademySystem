using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SportPriceCommands.CreateSportPrice;
using SportAcademy.Application.Commands.SportPriceCommands.DeleteSportPrice;
using SportAcademy.Application.Commands.SportPriceCommands.UpdateSportPrice;
using SportAcademy.Application.Queries.SportPriceQueries.GetAll;
using SportAcademy.Application.Queries.SportPriceQueries.GetById;

namespace SportAcademy.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SportPriceController : ControllerBase
	{
		private readonly IMediator _mediator;
		public SportPriceController(IMediator mediator)
		{
			_mediator = mediator;
		}
		[HttpPost("create")]
		public async Task<IActionResult> CreateSportPrice(CreateSportPriceCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpPut("update")]
		public async Task<IActionResult> UpdateSportPrice(UpdateSportPriceCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpDelete("delete")]
		public async Task<IActionResult> DeleteSportPrice(DeleteSportPriceCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpGet("get-all")]
		public async Task<IActionResult> GetAllSportPrices(CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(new GetAllSportPricesQuery(), cancellationToken);
			return Ok(result);
		}

		[HttpGet("get/{branchId}/{sportId}/{subsTypeId}")]
		public async Task<IActionResult> GetSportPriceByKey(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(new GetSportPriceByKeyQuery(branchId, sportId, subsTypeId), cancellationToken);
			return Ok(result);
		}



	}
}

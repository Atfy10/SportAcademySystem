using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SportPriceCommands.CreateSportPrice;
using SportAcademy.Application.Commands.SportPriceCommands.DeleteSportPrice;
using SportAcademy.Application.Commands.SportPriceCommands.UpdateSportPrice;
using SportAcademy.Application.Queries.SportPriceQueries.GetAll;
using SportAcademy.Application.Queries.SportPriceQueries.GetById;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
	[ApiController]
	public class SportPriceController : ControllerBase
	{
		private readonly IMediator _mediator;
		public SportPriceController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		public async Task<IActionResult> CreateSportPrice(CreateSportPriceCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpPut]
		public async Task<IActionResult> UpdateSportPrice(UpdateSportPriceCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteSportPrice(DeleteSportPriceCommand command, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(command, cancellationToken);
			return Ok(result);
		}

		[HttpGet]
		public async Task<IActionResult> GetAllSportPrices(CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(new GetAllSportPricesQuery(), cancellationToken);
			return Ok(result);
		}

		[HttpGet("branches/{branchId}/sports/{sportId}/subType/{subscriptionTypeId}")]
		public async Task<IActionResult> GetSportPriceByKey(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken)
		{
			var result = await _mediator.Send(new GetSportPriceByKeyQuery(branchId, sportId, subsTypeId), cancellationToken);
			return Ok(result);
		}



	}
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.ExpenseCategoryCommands.CreateExpenseCategory;
using SportAcademy.Application.Commands.ExpenseCategoryCommands.DeleteExpenseCategory;
using SportAcademy.Application.Commands.ExpenseCategoryCommands.UpdateExpenseCategory;
using SportAcademy.Application.Commands.ExpenseCommands.CreateExpense;
using SportAcademy.Application.Commands.ExpenseCommands.DeleteExpense;
using SportAcademy.Application.Commands.ExpenseCommands.UpdateExpense;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.ExpenseCategoryQueries.GetAll;
using SportAcademy.Application.Queries.ExpenseQueries.GetExpenseById;
using SportAcademy.Application.Queries.ExpenseQueries.GetExpenses;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/expenses")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExpensesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:expense.view")]
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllExpenseCategoriesQuery(), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:expense.manage")]
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateExpenseCategoryCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:expense.manage")]
        [HttpPut("categories")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateExpenseCategoryCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:expense.manage")]
        [HttpDelete("categories")]
        public async Task<IActionResult> DeleteCategory([FromBody] DeleteExpenseCategoryCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return NoContent();
        }

        [Authorize(Policy = "Permission:expense.view")]
        [HttpGet]
        public async Task<IActionResult> GetExpenses(
            [FromQuery] int? branchId, [FromQuery] int? expenseCategoryId,
            [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
            [FromQuery] int? page, [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetExpensesQuery(PageRequest.Create(page, pageSize), branchId, expenseCategoryId, from, to), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:expense.view")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetExpenseByIdQuery(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:expense.manage")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:expense.manage")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateExpenseCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:expense.manage")]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] DeleteExpenseCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return NoContent();
        }
    }
}

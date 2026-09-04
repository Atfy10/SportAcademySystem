using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;

namespace SportAcademy.Application.Queries.ExpenseCategoryQueries.GetAll
{
    public record GetAllExpenseCategoriesQuery() : IRequest<Result<List<ExpenseCategoryDto>>>;
}

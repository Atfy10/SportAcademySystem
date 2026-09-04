using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ExpenseCategoryQueries.GetAll
{
    public class GetAllExpenseCategoriesQueryHandler : IRequestHandler<GetAllExpenseCategoriesQuery, Result<List<ExpenseCategoryDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly IExpenseCategoryRepository _repository;

        public GetAllExpenseCategoriesQueryHandler(IExpenseCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<ExpenseCategoryDto>>> Handle(GetAllExpenseCategoriesQuery request, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);

            var dtos = entities
                .OrderBy(e => e.Name)
                .Select(e => new ExpenseCategoryDto(e.Id, e.Name, e.IsActive))
                .ToList();

            return Result<List<ExpenseCategoryDto>>.Success(dtos, _operation);
        }
    }
}

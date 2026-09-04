using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.ExpenseCategoryCommands.CreateExpenseCategory
{
    public class CreateExpenseCategoryCommandHandler : IRequestHandler<CreateExpenseCategoryCommand, Result<ExpenseCategoryDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly IExpenseCategoryRepository _repository;

        public CreateExpenseCategoryCommandHandler(IExpenseCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ExpenseCategoryDto>> Handle(CreateExpenseCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = new ExpenseCategory
            {
                Name = request.Name,
                IsActive = request.IsActive,
            };

            await _repository.AddAsync(entity, cancellationToken);

            return Result<ExpenseCategoryDto>.Success(
                new ExpenseCategoryDto(entity.Id, entity.Name, entity.IsActive), _operation);
        }
    }
}

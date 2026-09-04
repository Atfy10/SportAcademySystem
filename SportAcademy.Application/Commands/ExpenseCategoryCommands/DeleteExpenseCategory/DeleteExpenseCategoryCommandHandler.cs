using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.ExpenseExceptions;

namespace SportAcademy.Application.Commands.ExpenseCategoryCommands.DeleteExpenseCategory
{
    public class DeleteExpenseCategoryCommandHandler : IRequestHandler<DeleteExpenseCategoryCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly IExpenseCategoryRepository _repository;

        public DeleteExpenseCategoryCommandHandler(IExpenseCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteExpenseCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new ExpenseCategoryNotFoundException(request.Id.ToString());

            if (await _repository.AnyExpensesInCategoryAsync(entity.Id, cancellationToken))
                throw new ExpenseCategoryInUseException(entity.Id);

            await _repository.DeleteAsync(entity, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}

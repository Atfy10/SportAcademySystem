using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.ExpenseExceptions;

namespace SportAcademy.Application.Commands.ExpenseCommands.DeleteExpense
{
    public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly IExpenseRepository _repository;

        public DeleteExpenseCommandHandler(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new ExpenseNotFoundException(request.Id.ToString());

            await _repository.DeleteAsync(entity, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}

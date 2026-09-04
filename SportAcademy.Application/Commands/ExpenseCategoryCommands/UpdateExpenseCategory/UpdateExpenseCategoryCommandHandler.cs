using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.ExpenseExceptions;

namespace SportAcademy.Application.Commands.ExpenseCategoryCommands.UpdateExpenseCategory
{
    public class UpdateExpenseCategoryCommandHandler : IRequestHandler<UpdateExpenseCategoryCommand, Result<ExpenseCategoryDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IExpenseCategoryRepository _repository;

        public UpdateExpenseCategoryCommandHandler(IExpenseCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ExpenseCategoryDto>> Handle(UpdateExpenseCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new ExpenseCategoryNotFoundException(request.Id.ToString());

            if (request.Name is not null)
                entity.Name = request.Name;

            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;

            await _repository.UpdateAsync(entity, cancellationToken);

            return Result<ExpenseCategoryDto>.Success(
                new ExpenseCategoryDto(entity.Id, entity.Name, entity.IsActive), _operation);
        }
    }
}

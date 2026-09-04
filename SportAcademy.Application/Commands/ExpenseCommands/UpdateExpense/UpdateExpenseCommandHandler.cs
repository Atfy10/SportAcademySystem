using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.ExpenseExceptions;

namespace SportAcademy.Application.Commands.ExpenseCommands.UpdateExpense
{
    public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, Result<ExpenseDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IExpenseRepository _repository;
        private readonly IExpenseCategoryRepository _categoryRepository;
        private readonly IBranchRepository _branchRepository;

        public UpdateExpenseCommandHandler(
            IExpenseRepository repository,
            IExpenseCategoryRepository categoryRepository,
            IBranchRepository branchRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _branchRepository = branchRepository;
        }

        public async Task<Result<ExpenseDto>> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new ExpenseNotFoundException(request.Id.ToString());

            if (request.Title is not null)
                entity.Title = request.Title;

            if (request.ExpenseCategoryId.HasValue)
            {
                if (!await _categoryRepository.IsExistAsync(request.ExpenseCategoryId.Value, cancellationToken))
                    throw new ExpenseCategoryNotFoundException(request.ExpenseCategoryId.Value.ToString());

                entity.ExpenseCategoryId = request.ExpenseCategoryId.Value;
            }

            if (request.BranchId.HasValue)
            {
                if (!await _branchRepository.IsExistAsync(request.BranchId.Value, cancellationToken))
                    throw new BranchNotFoundException(request.BranchId.Value.ToString());

                entity.BranchId = request.BranchId.Value;
            }

            if (request.Amount.HasValue)
                entity.Amount = request.Amount.Value;

            if (request.ExpenseDate.HasValue)
                entity.ExpenseDate = request.ExpenseDate.Value;

            if (request.PaymentTypeId.HasValue)
                entity.PaymentTypeId = request.PaymentTypeId.Value;

            if (request.Notes is not null)
                entity.Notes = request.Notes;

            await _repository.UpdateAsync(entity, cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;

            return Result<ExpenseDto>.Success(ExpenseMapper.ToDto(saved), _operation);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.ExpenseExceptions;

namespace SportAcademy.Application.Commands.ExpenseCommands.CreateExpense
{
    public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<ExpenseDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly IExpenseRepository _repository;
        private readonly IExpenseCategoryRepository _categoryRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IUserContextService _userContext;

        public CreateExpenseCommandHandler(
            IExpenseRepository repository,
            IExpenseCategoryRepository categoryRepository,
            IBranchRepository branchRepository,
            IUserContextService userContext)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _branchRepository = branchRepository;
            _userContext = userContext;
        }

        public async Task<Result<ExpenseDto>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
        {
            if (!await _categoryRepository.IsExistAsync(request.ExpenseCategoryId, cancellationToken))
                throw new ExpenseCategoryNotFoundException(request.ExpenseCategoryId.ToString());

            if (!await _branchRepository.IsExistAsync(request.BranchId, cancellationToken))
                throw new BranchNotFoundException(request.BranchId.ToString());

            var entity = new Expense
            {
                Title = request.Title,
                ExpenseCategoryId = request.ExpenseCategoryId,
                BranchId = request.BranchId,
                Amount = request.Amount,
                ExpenseDate = request.ExpenseDate,
                PaymentTypeId = request.PaymentTypeId,
                Notes = request.Notes,
                RecordedByUserId = _userContext.UserId ?? Guid.Empty,
            };

            await _repository.AddAsync(entity, cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;

            return Result<ExpenseDto>.Success(ExpenseMapper.ToDto(saved), _operation);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SalaryPaymentExceptions;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.MarkSalaryPaymentPaid
{
    public class MarkSalaryPaymentPaidCommandHandler : IRequestHandler<MarkSalaryPaymentPaidCommand, Result<SalaryPaymentDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISalaryPaymentRepository _repository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;

        public MarkSalaryPaymentPaidCommandHandler(
            ISalaryPaymentRepository repository, IUserContextService userContext, IUserRepository userRepository)
        {
            _repository = repository;
            _userContext = userContext;
            _userRepository = userRepository;
        }

        public async Task<Result<SalaryPaymentDto>> Handle(MarkSalaryPaymentPaidCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SalaryPaymentNotFoundException(request.Id.ToString());

            if (entity.Status != SalaryPaymentStatus.Approved)
                throw new InvalidSalaryPaymentTransitionException(entity.Id, entity.Status.ToString(), "marked as paid");

            entity.Status = SalaryPaymentStatus.Paid;
            entity.PaidByUserId = _userContext.UserId;
            entity.PaidAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity, cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;
            var nameLookup = new Dictionary<Guid, string>();
            if (saved.ReviewedByUserId is { } reviewedBy)
                nameLookup[reviewedBy] = await _userRepository.GetDisplayNameAsync(reviewedBy, cancellationToken);
            if (saved.PaidByUserId is { } paidBy)
                nameLookup[paidBy] = await _userRepository.GetDisplayNameAsync(paidBy, cancellationToken);

            return Result<SalaryPaymentDto>.Success(SalaryPaymentMapper.ToDto(saved, nameLookup), _operation);
        }
    }
}

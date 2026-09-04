using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SalaryPaymentExceptions;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.ApproveSalaryPayment
{
    public class ApproveSalaryPaymentCommandHandler : IRequestHandler<ApproveSalaryPaymentCommand, Result<SalaryPaymentDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISalaryPaymentRepository _repository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;

        public ApproveSalaryPaymentCommandHandler(
            ISalaryPaymentRepository repository, IUserContextService userContext, IUserRepository userRepository)
        {
            _repository = repository;
            _userContext = userContext;
            _userRepository = userRepository;
        }

        public async Task<Result<SalaryPaymentDto>> Handle(ApproveSalaryPaymentCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SalaryPaymentNotFoundException(request.Id.ToString());

            if (entity.Status != SalaryPaymentStatus.PendingApproval)
                throw new InvalidSalaryPaymentTransitionException(entity.Id, entity.Status.ToString(), "approved");

            entity.Status = SalaryPaymentStatus.Approved;
            entity.ReviewedByUserId = _userContext.UserId;
            entity.ReviewedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity, cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;
            var nameLookup = await BuildNameLookupAsync(saved, cancellationToken);

            return Result<SalaryPaymentDto>.Success(SalaryPaymentMapper.ToDto(saved, nameLookup), _operation);
        }

        private async Task<Dictionary<Guid, string>> BuildNameLookupAsync(
            Domain.Entities.Finance.SalaryPayment sp, CancellationToken ct)
        {
            var lookup = new Dictionary<Guid, string>();
            if (sp.ReviewedByUserId is { } reviewedBy)
                lookup[reviewedBy] = await _userRepository.GetDisplayNameAsync(reviewedBy, ct);
            if (sp.PaidByUserId is { } paidBy)
                lookup[paidBy] = await _userRepository.GetDisplayNameAsync(paidBy, ct);
            return lookup;
        }
    }
}

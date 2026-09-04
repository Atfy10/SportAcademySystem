using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SalaryPaymentExceptions;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.RejectSalaryPayment
{
    public class RejectSalaryPaymentCommandHandler : IRequestHandler<RejectSalaryPaymentCommand, Result<SalaryPaymentDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISalaryPaymentRepository _repository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;

        public RejectSalaryPaymentCommandHandler(
            ISalaryPaymentRepository repository, IUserContextService userContext, IUserRepository userRepository)
        {
            _repository = repository;
            _userContext = userContext;
            _userRepository = userRepository;
        }

        public async Task<Result<SalaryPaymentDto>> Handle(RejectSalaryPaymentCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SalaryPaymentNotFoundException(request.Id.ToString());

            if (entity.Status != SalaryPaymentStatus.PendingApproval)
                throw new InvalidSalaryPaymentTransitionException(entity.Id, entity.Status.ToString(), "rejected");

            entity.Status = SalaryPaymentStatus.Rejected;
            entity.ReviewedByUserId = _userContext.UserId;
            entity.ReviewedAt = DateTime.UtcNow;
            entity.RejectionReason = request.RejectionReason;

            await _repository.UpdateAsync(entity, cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;
            var nameLookup = new Dictionary<Guid, string>();
            if (saved.ReviewedByUserId is { } reviewedBy)
                nameLookup[reviewedBy] = await _userRepository.GetDisplayNameAsync(reviewedBy, cancellationToken);

            return Result<SalaryPaymentDto>.Success(SalaryPaymentMapper.ToDto(saved, nameLookup), _operation);
        }
    }
}

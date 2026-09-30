using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.PaymentExceptions;

namespace SportAcademy.Application.Commands.PaymentCommands.UpdatePayment
{
    public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Result>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentTypeRepository _paymentTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _operation = OperationType.Update.ToString();

        public UpdatePaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IPaymentTypeRepository paymentTypeRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _paymentTypeRepository = paymentTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByIdAsync(request.PaymentNumber, cancellationToken)
                ?? throw new PaymentNotFoundException(request.PaymentNumber);

            // A voided / fully refunded payment is a closed record - re-dating it or changing its
            // method afterwards would rewrite history the refund entries were based on.
            if (payment.Status is PaymentStatus.Voided or PaymentStatus.Refunded)
                throw FinanceRuleException.ReversedPaymentLocked(payment.PaymentNumber);

            if (!await _paymentTypeRepository.IsExistAsync(request.PaymentTypeId, cancellationToken))
                throw new IdNotFoundException("PaymentType", request.PaymentTypeId.ToString());

            payment.PaymentTypeId = request.PaymentTypeId;
            payment.PaidDate = request.PaidDate;

            await _paymentRepository.UpdateAsyncWithoutSave(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(_operation);
        }
    }
}

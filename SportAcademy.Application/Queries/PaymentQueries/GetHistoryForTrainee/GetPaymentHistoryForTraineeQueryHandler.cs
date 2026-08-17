using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PaymentQueries.GetHistoryForTrainee
{
    public class GetPaymentHistoryForTraineeQueryHandler : IRequestHandler<GetPaymentHistoryForTraineeQuery, Result<List<PaymentHistoryDto>>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly string _operation = OperationType.Get.ToString();

        public GetPaymentHistoryForTraineeQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<List<PaymentHistoryDto>>> Handle(GetPaymentHistoryForTraineeQuery request, CancellationToken cancellationToken)
        {
            var history = await _paymentRepository.GetHistoryForTraineeAsync(request.TraineeId, cancellationToken);
            return Result<List<PaymentHistoryDto>>.Success(history, _operation);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentDtos;

namespace SportAcademy.Application.Queries.PaymentQueries.GetHistoryForTrainee
{
    public record GetPaymentHistoryForTraineeQuery(int TraineeId) : IRequest<Result<List<PaymentHistoryDto>>>;
}

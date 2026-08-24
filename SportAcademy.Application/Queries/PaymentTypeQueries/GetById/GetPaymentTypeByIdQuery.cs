using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;

namespace SportAcademy.Application.Queries.PaymentTypeQueries.GetById
{
    public record GetPaymentTypeByIdQuery(int Id) : IRequest<Result<PaymentTypeDto>>;
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;

namespace SportAcademy.Application.Queries.PaymentTypeQueries.GetAll
{
    public record GetAllPaymentTypesQuery() : IRequest<Result<List<PaymentTypeDto>>>;
}

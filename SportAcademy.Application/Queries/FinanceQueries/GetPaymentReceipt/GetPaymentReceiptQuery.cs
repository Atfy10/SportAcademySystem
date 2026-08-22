using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.FinanceQueries.GetPaymentReceipt;

public record GetPaymentReceiptQuery(string PaymentNumber) : IRequest<Result<PaymentReceiptDto>>;

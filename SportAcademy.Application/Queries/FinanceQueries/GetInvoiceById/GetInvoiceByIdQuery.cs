using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.FinanceQueries.GetInvoiceById;

public record GetInvoiceByIdQuery(int Id) : IRequest<Result<InvoiceDto>>;

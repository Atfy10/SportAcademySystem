using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.SalaryPaymentQueries.GetSalaryPaymentById;

public record GetSalaryPaymentByIdQuery(int Id) : IRequest<Result<SalaryPaymentDto>>;

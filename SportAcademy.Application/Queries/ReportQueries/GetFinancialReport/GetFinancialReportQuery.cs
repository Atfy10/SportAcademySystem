using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.ReportQueries.GetFinancialReport;

public record GetFinancialReportQuery(DateTime? From, DateTime? To, int? BranchId)
    : IRequest<Result<List<FinancialReportRow>>>;

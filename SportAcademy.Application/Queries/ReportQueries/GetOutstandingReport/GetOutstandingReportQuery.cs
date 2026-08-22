using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.ReportQueries.GetOutstandingReport;

public record GetOutstandingReportQuery(int? BranchId) : IRequest<Result<OutstandingReportSummary>>;

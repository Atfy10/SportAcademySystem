using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.ReportQueries.GetRevenueReport;

// GroupBy: "month" (default) or "branch". See RevenueReportRow remarks for why "sport" isn't
// implemented yet.
public record GetRevenueReportQuery(DateTime? From, DateTime? To, int? BranchId, string? GroupBy)
    : IRequest<Result<List<RevenueReportRow>>>;

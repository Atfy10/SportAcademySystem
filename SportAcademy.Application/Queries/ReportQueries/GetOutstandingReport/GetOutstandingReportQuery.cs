using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.ReportQueries.GetOutstandingReport;

public record GetOutstandingReportQuery(int? BranchId) : IRequest<Result<OutstandingReportSummary>>, IRequiresFeature
{
    public string FeatureKey => "financial-reports";
}

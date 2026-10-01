using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.ReportQueries.GetFinancialStatement;

public record GetFinancialStatementQuery(DateTime? From, DateTime? To, int? BranchId)
    : IRequest<Result<FinancialStatementDto>>, IRequiresFeature
{
    public string FeatureKey => "financial-reports";
}

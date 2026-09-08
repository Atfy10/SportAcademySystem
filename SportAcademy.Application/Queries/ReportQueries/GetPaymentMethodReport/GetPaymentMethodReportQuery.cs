using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.ReportQueries.GetPaymentMethodReport;

public record GetPaymentMethodReportQuery(DateTime? From, DateTime? To, int? BranchId)
    : IRequest<Result<List<PaymentMethodReportRow>>>, IRequiresFeature
{
    public string FeatureKey => "financial-reports";
}

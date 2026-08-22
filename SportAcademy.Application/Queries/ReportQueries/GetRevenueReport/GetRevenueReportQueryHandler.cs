using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ReportQueries.GetRevenueReport;

public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, Result<List<RevenueReportRow>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetRevenueReportQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<List<RevenueReportRow>>> Handle(GetRevenueReportQuery request, CancellationToken ct)
    {
        var rows = string.Equals(request.GroupBy, "branch", StringComparison.OrdinalIgnoreCase)
            ? await _paymentRepository.GetRevenueByBranchAsync(request.From, request.To, request.BranchId, ct)
            : await _paymentRepository.GetRevenueByMonthAsync(request.From, request.To, request.BranchId, ct);

        var dtos = rows.Select(r => new RevenueReportRow(
            r.GroupKey, r.Gross, r.Refunded, r.Gross - r.Refunded, r.Count)).ToList();

        return Result<List<RevenueReportRow>>.Success(dtos, _operation);
    }
}

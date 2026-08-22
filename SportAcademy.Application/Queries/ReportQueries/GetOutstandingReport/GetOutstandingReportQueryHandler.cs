using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ReportQueries.GetOutstandingReport;

public class GetOutstandingReportQueryHandler : IRequestHandler<GetOutstandingReportQuery, Result<OutstandingReportSummary>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetOutstandingReportQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<OutstandingReportSummary>> Handle(GetOutstandingReportQuery request, CancellationToken ct)
    {
        var (total, count, overdueCount, overdueAmount) =
            await _invoiceRepository.GetOutstandingSummaryAsync(request.BranchId, ct);

        return Result<OutstandingReportSummary>.Success(
            new OutstandingReportSummary(total, count, overdueCount, overdueAmount), _operation);
    }
}

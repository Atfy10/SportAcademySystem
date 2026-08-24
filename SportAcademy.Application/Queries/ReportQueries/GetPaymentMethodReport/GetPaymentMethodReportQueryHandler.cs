using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ReportQueries.GetPaymentMethodReport;

public class GetPaymentMethodReportQueryHandler : IRequestHandler<GetPaymentMethodReportQuery, Result<List<PaymentMethodReportRow>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetPaymentMethodReportQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<List<PaymentMethodReportRow>>> Handle(GetPaymentMethodReportQuery request, CancellationToken ct)
    {
        var rows = await _paymentRepository.GetPaymentMethodBreakdownAsync(request.From, request.To, request.BranchId, ct);

        var dtos = rows.Select(r => new PaymentMethodReportRow(r.PaymentTypeName, r.Total, r.Count)).ToList();

        return Result<List<PaymentMethodReportRow>>.Success(dtos, _operation);
    }
}

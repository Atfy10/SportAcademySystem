using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ReportQueries.GetSubscriptionsReport;

public class GetSubscriptionsReportQueryHandler : IRequestHandler<GetSubscriptionsReportQuery, Result<PagedData<SubscriptionDetailsDto>>>
{
    private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetSubscriptionsReportQueryHandler(ISubscriptionDetailsRepository subscriptionDetailsRepository)
    {
        _subscriptionDetailsRepository = subscriptionDetailsRepository;
    }

    public async Task<Result<PagedData<SubscriptionDetailsDto>>> Handle(GetSubscriptionsReportQuery request, CancellationToken ct)
    {
        Enum.TryParse<SubscriptionStatus>(request.Status, ignoreCase: true, out var parsedStatus);
        SubscriptionStatus? status = string.IsNullOrWhiteSpace(request.Status) ? null : parsedStatus;

        var data = await _subscriptionDetailsRepository.GetReportAsync(
            request.From, request.To, request.BranchId, request.SportId, status, request.Page, ct);

        return Result<PagedData<SubscriptionDetailsDto>>.Success(data, _operation);
    }
}

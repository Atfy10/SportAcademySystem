using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAllForDropdown;

public class GetAllSubscriptionDetailsForDropdownQueryHandler(
    ISubscriptionDetailsRepository subscriptionDetailsRepository)
    : IRequestHandler<GetAllSubscriptionDetailsForDropdownQuery, Result<List<SubscriptionDetailsDropdownDto>>>
{
    public async Task<Result<List<SubscriptionDetailsDropdownDto>>> Handle(
        GetAllSubscriptionDetailsForDropdownQuery request,
        CancellationToken cancellationToken)
    {
        var items = await subscriptionDetailsRepository.GetAllForDropdownAsync(cancellationToken);
        return Result<List<SubscriptionDetailsDropdownDto>>.Success(items, OperationType.GetAll.ToString());
    }
}

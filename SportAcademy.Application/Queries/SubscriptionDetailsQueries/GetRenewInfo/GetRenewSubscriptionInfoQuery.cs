using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetRenewInfo
{
    public record GetRenewSubscriptionInfoQuery(int Id) : IRequest<Result<RenewSubscriptionInfoDto>>;
}

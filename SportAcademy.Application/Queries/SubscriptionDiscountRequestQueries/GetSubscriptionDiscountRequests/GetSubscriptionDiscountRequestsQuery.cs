using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;

namespace SportAcademy.Application.Queries.SubscriptionDiscountRequestQueries.GetSubscriptionDiscountRequests;

public record GetSubscriptionDiscountRequestsQuery(
    PageRequest Page, string? Status, int? BranchId
) : IRequest<Result<PagedData<SubscriptionDiscountRequestDto>>>;

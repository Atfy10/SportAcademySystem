using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionDiscountRequestQueries.GetSubscriptionDiscountRequests;

public class GetSubscriptionDiscountRequestsQueryHandler
    : IRequestHandler<GetSubscriptionDiscountRequestsQuery, Result<PagedData<SubscriptionDiscountRequestDto>>>
{
    private readonly ISubscriptionDiscountRequestRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetSubscriptionDiscountRequestsQueryHandler(
        ISubscriptionDiscountRequestRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<Result<PagedData<SubscriptionDiscountRequestDto>>> Handle(
        GetSubscriptionDiscountRequestsQuery request, CancellationToken ct)
    {
        SubscriptionDiscountRequestStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<SubscriptionDiscountRequestStatus>(request.Status, true, out var parsed))
            status = parsed;

        var (items, totalCount) = await _repository.GetPagedAsync(request.Page, status, request.BranchId, ct);

        // Resolve each distinct requester/reviewer id once, not once per row.
        var distinctUserIds = items
            .SelectMany(r => new[] { (Guid?)r.RequestedByUserId, r.ReviewedByUserId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var nameLookup = new Dictionary<Guid, string>();
        foreach (var userId in distinctUserIds)
            nameLookup[userId] = await _userRepository.GetDisplayNameAsync(userId, ct);

        var dtos = items.Select(r => SubscriptionDiscountRequestMapper.ToDto(r, nameLookup)).ToList();

        return Result<PagedData<SubscriptionDiscountRequestDto>>.Success(new PagedData<SubscriptionDiscountRequestDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page.Page,
            PageSize = request.Page.PageSize,
        }, _operation);
    }
}

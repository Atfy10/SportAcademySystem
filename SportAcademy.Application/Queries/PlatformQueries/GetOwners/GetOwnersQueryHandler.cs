using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetOwners;

public class GetOwnersQueryHandler : IRequestHandler<GetOwnersQuery, Result<PagedData<OwnerListItemDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetOwnersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedData<OwnerListItemDto>>> Handle(GetOwnersQuery request, CancellationToken ct)
    {
        var page = PageRequest.Create(request.Page, request.PageSize);
        var (items, totalCount) = await _userRepository.GetOwnersPagedAsync(
            page.Skip, page.PageSize, request.Search, ct);

        var data = new PagedData<OwnerListItemDto>
        {
            Items = items.Select(u => new OwnerListItemDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt,
                TenantId = u.TenantId,
                TenantName = u.Tenant.DisplayName,
                TenantStatus = u.Tenant.Status.ToString(),
            }).ToList(),
            TotalCount = totalCount,
            Page = page.Page,
            PageSize = page.PageSize
        };

        return Result<PagedData<OwnerListItemDto>>.Success(data, _operation);
    }
}

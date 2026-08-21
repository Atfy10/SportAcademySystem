using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetOwnerById;

public class GetOwnerByIdQueryHandler : IRequestHandler<GetOwnerByIdQuery, Result<OwnerDetailDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetOwnerByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<OwnerDetailDto>> Handle(GetOwnerByIdQuery request, CancellationToken ct)
    {
        var owner = await _userRepository.GetOwnerByIdAsync(request.OwnerId, ct);
        if (owner is null)
            return Result<OwnerDetailDto>.Failure(_operation, "Owner not found.", 404);

        var dto = new OwnerDetailDto
        {
            Id = owner.Id,
            UserName = owner.UserName,
            Email = owner.Email,
            PhoneNumber = owner.PhoneNumber,
            IsBanned = owner.IsBanned,
            EmailConfirmed = owner.EmailConfirmed,
            CreatedAt = owner.CreatedAt,
            TenantId = owner.TenantId,
            TenantName = owner.Tenant.Name,
            TenantDisplayName = owner.Tenant.DisplayName,
            TenantStatus = owner.Tenant.Status.ToString(),
            TenantSlug = owner.Tenant.Slug,
        };

        return Result<OwnerDetailDto>.Success(dto, _operation);
    }
}

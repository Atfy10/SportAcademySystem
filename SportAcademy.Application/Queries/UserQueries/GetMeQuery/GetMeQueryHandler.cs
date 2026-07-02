using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.UserQueries.GetMeQuery;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<MeResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public GetMeQueryHandler(IUserRepository userRepository, IUserContextService userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<MeResponse>> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userId = _userContext.UserId;
        if (userId is null)
            return Result<MeResponse>.Failure(_operation, "User ID is not available in the context.", 400);

        var user = await _userRepository.GetByIdAsync(userId.Value, ct)
            ?? throw new IdNotFoundException(nameof(AppUser), userId);

        var roles = await _userRepository.GetUserRoleAsync(user, ct);
        var rolesList = roles.ToList();

        var response = new MeResponse
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            Roles = rolesList!,
            CreatedAt = user.CreatedAt
        };

        return Result<MeResponse>.Success(response, _operation);
    }
}

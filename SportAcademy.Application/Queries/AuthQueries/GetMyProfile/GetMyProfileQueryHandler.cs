using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.AuthQueries.GetMyProfile;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, Result<MyProfileDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public GetMyProfileQueryHandler(IUserRepository userRepository, IUserContextService userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<MyProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(_userContext.UserId), cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), _userContext.UserId);

        var roles = await _userRepository.GetUserRoleAsync(user, cancellationToken);

        var profile = new MyProfileDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            Roles = (List<string>)roles,
            CreatedAt = user.CreatedAt
        };

        return Result<MyProfileDto>.Success(profile, _operation);
    }
}

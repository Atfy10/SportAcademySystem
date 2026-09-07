using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.ProfileCommands.UpdateMyProfile;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, Result<MeResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IUserContextService _userContext;
    private readonly IFileStorageService _fileStorage;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateMyProfileCommandHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IUserContextService userContext,
        IFileStorageService fileStorage)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _userContext = userContext;
        _fileStorage = fileStorage;
    }

    public async Task<Result<MeResponse>> Handle(UpdateMyProfileCommand request, CancellationToken ct)
    {
        var userId = _userContext.UserId;
        if (userId is null)
            return Result<MeResponse>.Failure(_operation, "User ID is not available in the context.", 400);

        var user = await _userRepository.GetByIdAsync(userId.Value, ct)
            ?? throw new IdNotFoundException(nameof(AppUser), userId);

        if (request.PhoneNumber is not null)
        {
            user.PhoneNumber = request.PhoneNumber;
            await _userRepository.UpdateAsync(user, ct);
        }

        // Same "create on the spot" fallback CompleteOnboardingCommandHandler uses - every
        // AppUser is meant to get a companion Profile row at creation time, but this table
        // predates that guarantee being consistently enforced across every creation path.
        var profile = await _profileRepository.GetByAppUserIdAsync(userId.Value, ct);
        var isNewProfile = profile is null;
        profile ??= new Profile { AppUserId = userId.Value };

        if (request.ProfileImageUrl is not null && request.ProfileImageUrl != profile.ProfileImageUrl)
        {
            // Best-effort cleanup of the old file - never blocks the actual profile update if
            // it fails (see LocalFileStorageService.DeleteImage).
            _fileStorage.DeleteImage(profile.ProfileImageUrl);
            profile.ProfileImageUrl = request.ProfileImageUrl;
        }

        if (request.Bio is not null)
            profile.Bio = request.Bio;

        if (isNewProfile)
            await _profileRepository.AddAsync(profile, ct);
        else
            await _profileRepository.UpdateAsync(profile, ct);

        var roles = await _userRepository.GetUserRoleAsync(user, ct);

        var response = new MeResponse
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            Roles = roles!.ToList()!,
            CreatedAt = user.CreatedAt,
            HasCompletedOnboarding = profile.HasCompletedOnboarding,
            PreferredLanguage = profile.PreferredLanguage,
            ProfileImageUrl = profile.ProfileImageUrl,
            Bio = profile.Bio,
        };

        return Result<MeResponse>.Success(response, _operation, "Profile updated successfully.");
    }
}

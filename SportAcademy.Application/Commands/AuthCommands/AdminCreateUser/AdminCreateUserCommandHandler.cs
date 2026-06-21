using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos.AdminDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System.Security.Cryptography;

namespace SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;

public class AdminCreateUserCommandHandler : IRequestHandler<AdminCreateUserCommand, Result<AdminCreateUserResultDto>>
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly string _operationType = OperationType.Add.ToString();

    public AdminCreateUserCommandHandler(
        IMapper mapper,
        IUserRepository userRepository,
        IProfileRepository profileRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
    }

    public async Task<Result<AdminCreateUserResultDto>> Handle(AdminCreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.IsUsernameExistAsync(request.UserName, cancellationToken))
        {
            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                $"Username '{request.UserName}' is already taken.",
                400,
                new Dictionary<string, string[]>
                {
                    ["UserName"] = [$"Username '{request.UserName}' is already taken."]
                });
        }

        if (await _userRepository.IsEmailExistAsync(request.Email, cancellationToken))
        {
            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                $"Email '{request.Email}' is already registered.",
                400,
                new Dictionary<string, string[]>
                {
                    ["Email"] = [$"Email '{request.Email}' is already registered."]
                });
        }

        var user = _mapper.Map<AppUser>(request)
            ?? throw new AutoMapperMappingException("Error occurred while mapping.");

        user.IsBanned = !request.IsActive;
        user.EmailConfirmed = request.EmailConfirmed;

        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(8));

        var identityResult = await _userRepository.Register(user, password);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                "Failed to create user.",
                400,
                errors);
        }

        var profile = new Domain.Entities.Profile
        {
            AppUserId = user.Id,
        };

        await _profileRepository.AddAsync(profile, cancellationToken);

        return Result<AdminCreateUserResultDto>.Success(
            new AdminCreateUserResultDto
            {
                UserId = user.Id,
                UserName = user.UserName!,
                GeneratedPassword = password
            },
            _operationType);
    }
}

using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
    {
        private readonly string _operation = OperationType.Signup.ToString();
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IMapper mapper,
            IJwtTokenService jwtTokenService,
            IProfileRepository profileRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtTokenService = jwtTokenService;
            _profileRepository = profileRepository;
        }

        public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken ct)
        {
            var isUserNameExist = await _userRepository.IsUsernameExistAsync(request.UserName, ct);
            var isEmailExist = await _userRepository.IsEmailExistAsync(request.Email, ct);

            if (isUserNameExist)
                throw new UserNameExistException();

            if (isEmailExist)
                throw new EmailExistException();

            var user = _mapper.Map<AppUser>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            ct.ThrowIfCancellationRequested();

            var identityResult = await _userRepository.Register(user, request.Password);
            if (!identityResult.Succeeded)
                throw new UserRegistrationException(identityResult.Errors.Select(e => e.Description).ToList());

            ct.ThrowIfCancellationRequested();

            var token = _jwtTokenService.GenerateJwtToken(user, "User");

            ct.ThrowIfCancellationRequested();

            var profile = new Domain.Entities.Profile
            {
                AppUserId = user.Id,
            };

            await _profileRepository.AddAsync(profile, ct);

            return Result<string>.Success(token, _operation);
        }
    }
}

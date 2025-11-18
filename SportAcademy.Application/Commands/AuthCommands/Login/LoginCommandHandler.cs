using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly string _operation = OperationType.Login.ToString();
        private readonly IMapper _mapper;

        public LoginCommandHandler(
            IJwtTokenService jwtTokenService,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMapper mapper)
        {
            _jwtTokenService = jwtTokenService;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(LoginCommand request, CancellationToken ct)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UserNameOrEmail, ct)
                ?? throw new UserLoginException();

            var isPasswordValid = await _userRepository.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                throw new UserLoginException();

            var roles = await _roleRepository.GetRolesForUser(user.Id, ct);

            var token = _jwtTokenService.GenerateJwtToken(user, [.. roles]);

            return Result<string>.Success(token, _operation);
        }
    }
}

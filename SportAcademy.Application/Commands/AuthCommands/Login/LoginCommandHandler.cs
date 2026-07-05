using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly ITenantRepository _tenantRepository;
        private readonly string _operation = OperationType.Login.ToString();
        private const int RefreshTokenExpiryDays = 7;

        public LoginCommandHandler(
            IJwtTokenService jwtTokenService,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IMapper mapper,
            ITenantIdProvider tenantIdProvider,
            ITenantRepository tenantRepository)
        {
            _jwtTokenService = jwtTokenService;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tenantIdProvider = tenantIdProvider;
            _tenantRepository = tenantRepository;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var slug = request.Slug ?? "system";
            var tenant = await _tenantRepository.GetBySlugAsync(slug, cancellationToken);
            if (tenant == null)
                throw new UserLoginException();

            _tenantIdProvider.SetTenantId(tenant.Id);
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UserNameOrEmail, cancellationToken)
                ?? throw new UserLoginException();

            if (user.IsBanned)
                throw new UserLoginException();

            var isPasswordValid = await _userRepository.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                throw new UserLoginException();

            var roles = await _roleRepository.GetRolesForUser(user.Id, cancellationToken);

            var accessToken = _jwtTokenService.GenerateJwtToken(user, [.. roles]);

            var plainRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var refreshTokenHash = _jwtTokenService.HashToken(plainRefreshToken);

            var refreshTokenEntity = new Domain.Entities.RefreshToken
            {
                TokenHash = refreshTokenHash,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            return Result<AuthResponseDto>.Success(new AuthResponseDto(accessToken, plainRefreshToken), _operation);
        }
    }
}

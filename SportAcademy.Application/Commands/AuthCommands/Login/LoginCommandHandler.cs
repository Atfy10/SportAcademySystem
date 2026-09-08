using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;
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
        private readonly IMediator _mediator;
        private readonly string _operation = OperationType.Login.ToString();
        private const int RefreshTokenExpiryDays = 7;

        public LoginCommandHandler(
            IJwtTokenService jwtTokenService,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IMapper mapper,
            ITenantIdProvider tenantIdProvider,
            ITenantRepository tenantRepository,
            IMediator mediator)
        {
            _jwtTokenService = jwtTokenService;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tenantIdProvider = tenantIdProvider;
            _tenantRepository = tenantRepository;
            _mediator = mediator;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var slug = request.Slug ?? "system";
            var tenant = await _tenantRepository.GetBySlugAsync(slug, cancellationToken);
            if (tenant == null)
                throw new UserLoginException();

            // A suspended/archived/deactivated tenant should refuse a brand new login outright,
            // not just let TenantStatusGuardMiddleware reject the request that follows it - see
            // that middleware for the enforcement that applies to an already-issued token.
            if (tenant.Status is not TenantStatus.Active)
                throw new UserLoginException();

            // Deliberately not the System tenant: a SuperAdmin's own first login has no other
            // SuperAdmin to notify, and isn't the "a tenant started using the product" milestone
            // this event exists to surface. Invite acceptance (AcceptInvitationCommandHandler)
            // already hands out a live session before this is ever null, so this only fires on a
            // genuinely later, separate login-form use.
            var isFirstLogin = tenant.FirstLoginAt is null && tenant.Code != Tenant.SystemTenantCode;

            _tenantIdProvider.SetTenantId(tenant.Id);
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UserNameOrEmail, cancellationToken)
                ?? throw new UserLoginException();

            if (user.IsBanned)
                throw new UserLoginException();

            var isPasswordValid = await _userRepository.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                throw new UserLoginException();

            var roles = await _roleRepository.GetRolesForUser(user.Id, cancellationToken);

            var accessToken = await _jwtTokenService.GenerateJwtToken(user, [.. roles]);

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

            // Credentials are confirmed valid at this point - safe to mark the milestone now.
            // Set on the same tracked `tenant` before AddAsync below so its own SaveChanges
            // flushes both in one write; no extra SaveChanges call needed in this handler.
            if (isFirstLogin)
                tenant.FirstLoginAt = DateTime.UtcNow;

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            if (isFirstLogin)
                await _mediator.Publish(new FirstTenantLoginEvent(tenant.Id, tenant.DisplayName, user.Id), cancellationToken);

            return Result<AuthResponseDto>.Success(new AuthResponseDto(accessToken, plainRefreshToken), _operation);
        }
    }
}

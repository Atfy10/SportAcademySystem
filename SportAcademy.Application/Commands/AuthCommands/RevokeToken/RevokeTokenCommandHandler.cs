using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.RevokeToken
{
    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly string _operation = OperationType.RevokeToken.ToString();

        public RevokeTokenCommandHandler(IJwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken ct)
        {
            var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);
            var storedToken = await _jwtTokenService.GetRefreshTokenByHashAsync(tokenHash, ct);

            if (storedToken == null)
            {
                return Result<bool>.Failure(_operation, "Token not found", 404);
            }

            if (storedToken.IsRevoked)
            {
                return Result<bool>.Failure(_operation, "Token already revoked", 400);
            }

            storedToken.Revoke();
            await _jwtTokenService.RevokeRefreshTokenAsync(storedToken, ct);

            return Result<bool>.Success(true, _operation);
        }
    }
}

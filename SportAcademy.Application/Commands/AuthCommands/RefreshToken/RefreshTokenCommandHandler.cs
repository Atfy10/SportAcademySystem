using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly string _operation = OperationType.RefreshToken.ToString();

        public RefreshTokenCommandHandler(IJwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
        {
            var result = await _jwtTokenService.ValidateAndRefreshTokenAsync(request.RefreshToken, ct);

            if (result is null)
            {
                return Result<AuthResponseDto>.Failure(_operation, "Invalid or expired refresh token", 401);
            }

            return Result<AuthResponseDto>.Success(new AuthResponseDto(result.AccessToken, result.RefreshToken), _operation);
        }
    }
}

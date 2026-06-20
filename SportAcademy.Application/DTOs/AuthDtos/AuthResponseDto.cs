using System;

namespace SportAcademy.Application.DTOs.AuthDtos
{
    public record AuthResponseDto(string AccessToken, string RefreshToken);
    
    public record RefreshTokenRequest(string RefreshToken);
}

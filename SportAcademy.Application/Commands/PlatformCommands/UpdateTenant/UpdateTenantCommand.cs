using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateTenant;

public record UpdateTenantCommand(
    Guid TenantId,
    string? Name = null,
    string? DisplayName = null,
    string? Email = null,
    string? Phone = null,
    string? Address = null,
    string? Website = null,
    string? Description = null,
    string? TimeZone = null,
    string? Language = null,
    string? Currency = null
) : IRequest<Result<TenantDetailResponse>>;

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Commands.PlatformCommands.CreateTenant;

public record CreateTenantCommand(
    string Name,
    string DisplayName,
    string Slug,
    string Code,
    string Email,
    string OwnerName,
    string OwnerEmail,
    int SubscriptionPlanId,
    string? Phone = null,
    string? Address = null,
    string? TimeZone = null,
    string? Language = null,
    string? Currency = null
) : IRequest<Result<TenantDetailResponse>>;

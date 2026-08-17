using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.DTOs.TenantDtos;

public record UpdateTenantSettingsCommand(
    string? TimeZone,
    string? Language,
    string? DateFormat,
    string? TimeFormat,
    string? Currency
) : IRequest<Result>;

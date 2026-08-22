using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.DTOs.TenantDtos;

public record UpdateTenantProfileCommand(
    string? OrganizationName,
    string? LogoUrl,
    string? Email,
    string? Phone,
    string? Website,
    string? Address,
    string? TaxNumber,
    string? CommercialRegistration,
    string? Description
) : IRequest<Result>;

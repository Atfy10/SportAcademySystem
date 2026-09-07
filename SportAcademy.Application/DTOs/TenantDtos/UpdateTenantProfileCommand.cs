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
    string? Description,
    // Set only by the post-invite "complete your academy profile" onboarding wizard's own
    // submit action - an ordinary later edit from Settings never sends this, so re-saving the
    // profile afterward can't accidentally flip it (it's already true by then anyway).
    bool MarkSetupComplete = false
) : IRequest<Result>;

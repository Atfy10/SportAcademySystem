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
    bool MarkSetupComplete = false,
    // The tenant's one and only chance to choose its country (ISO 3166-1 alpha-2) - sent by that
    // same onboarding wizard submit, alongside MarkSetupComplete. The handler rejects this once
    // the profile's IsSetupComplete is already true, so it can never be changed afterward (not
    // even by the tenant itself) - see RegionalSettingsCard, which renders it read-only for
    // exactly this reason.
    string? Country = null
) : IRequest<Result>;

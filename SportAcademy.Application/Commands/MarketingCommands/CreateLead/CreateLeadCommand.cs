using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.MarketingCommands.CreateLead;

// Anonymous submission from the public marketing site's demo/contact form. Not IAuditableCommand
// - the platform audit trail is for admin actions on tenants, not public form submissions; a
// Lead's own CreatedAt/Status is its own record of what happened.
public record CreateLeadCommand(
    string FullName,
    string AcademyName,
    string Email,
    string PhoneNumber,
    string? City,
    int? BranchCount,
    int? TraineeCountBand,
    string? Message,
    string Locale,
    string? SourcePage,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? Referrer,
    string? IpHash,
    // Anti-spam signals, evaluated by the handler (not the controller) so the decision path is
    // unit-testable without an HTTP pipeline.
    string? HoneypotValue,
    DateTime? FormRenderedAtUtc
) : IRequest<Result<Guid>>;

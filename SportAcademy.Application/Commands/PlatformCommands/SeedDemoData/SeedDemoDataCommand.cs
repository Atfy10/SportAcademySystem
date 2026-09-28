using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.SeedDemoData;

/// <summary>Seeds the fictional demo tenant on demand. Development only.</summary>
public record SeedDemoDataCommand : IRequest<Result<SeedDemoDataDto>>, IAuditableCommand
{
    // Audited like every other platform command; the failed outcomes ("already seeded", "not
    // development") are recorded too, as Outcome = Failed.
    public string AuditEventType => "demo.seed";

    /// <summary>Set by the handler once the demo tenant is known - it doesn't exist yet when the
    /// command is sent, same pattern as CreateTenantCommand. Null when nothing was resolved
    /// (e.g. not Development).</summary>
    public Guid? ResolvedTenantId { get; set; }

    Guid? IAuditableCommand.AuditTenantId => ResolvedTenantId;

    // Creates a brand-new tenant (or changes nothing): there is no "before".
    object? IAuditableCommand.AuditBeforeState => null;
}

/// <param name="TenantSlug">Log in at <c>/t/{TenantSlug}/login</c>.</param>
/// <param name="OwnerUserName">User name of the demo Owner.</param>
public record SeedDemoDataDto(string TenantSlug, string OwnerUserName);

/// <summary>
/// Stable machine-readable codes for the two non-success outcomes - the frontend maps them to
/// its own localized messages, so nothing should match on the English text.
/// </summary>
public static class DemoDataErrorCodes
{
    public const string AlreadySeeded = "errors.demoData.alreadySeeded";
    public const string NotDevelopment = "errors.demoData.notDevelopment";
}

using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Marketing;

// Deliberately NOT ITenantScoped: a lead exists before any tenant does. Implementing that
// interface would put it behind the global tenant query filter and make it unreadable by
// design - see ApplicationDbContext.OnModelCreating, which applies the filter to every
// ITenantScoped type. Public marketing site + SuperAdmin platform console are the only two
// things that ever touch this table.
public class Lead
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;
    public string AcademyName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? City { get; set; }
    public int? BranchCount { get; set; }
    public int? TraineeCountBand { get; set; }
    public string? Message { get; set; }

    // Which language tree on the marketing site the lead was submitted from - lets the sales
    // reply and the auto-acknowledgement email be sent in the right language.
    public string Locale { get; set; } = "en";

    public string? SourcePage { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? Referrer { get; set; }

    // SHA-256(ip + a server-side salt), never the raw IP - enough to spot one client hammering
    // the endpoint (rate-limit correlation, abuse review) without storing PII we don't need.
    public string? IpHash { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;
    public string? InternalNotes { get; set; }

    // Set once a SuperAdmin turns this lead into a real tenant from the platform console -
    // makes the funnel measurable end to end (which campaign produced which paying academy).
    public Guid? ConvertedTenantId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ContactedAt { get; set; }
}

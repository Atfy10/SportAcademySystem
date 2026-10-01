using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Events;

// A booking of one branch's venue by an EventCustomer. Billed through the normal finance ledger:
// one Invoice per event (InvoiceId), with an EventFee line and, when decorated, an
// EventDecoration line - so its money shows up in payments, outstanding balances and revenue
// exactly like subscription money does.
//
// StartsAt/EndsAt are UTC instants, like every other stored DateTime (UtcDateTimeConverter).
// Staff enter and read them in the academy's own time zone (TenantSettings.TimeZone): the
// handlers convert on the way in (TenantCalendar.ToUtc) and the DTO carries a local copy on the
// way out (StartsAtLocal/EndsAtLocal). Upcoming / Ongoing / Completed is derived from them on
// read, on the academy's calendar (see EventStatusRules); only the cancellation is stored.
public class Event : ITenantScoped, IBranchScoped, IAuditableEntity, ISoftDeletable
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int BranchId { get; set; }
    public int EventCustomerId { get; set; }

    public bool WithDecorations { get; set; }
    // The rental price the creator set, and the separate decoration charge (0 without decorations).
    public decimal Price { get; set; }
    public decimal DecorationFee { get; set; }
    public int Capacity { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string? Notes { get; set; }

    // Who booked it - required and never Guid.Empty (the handler refuses a request with no
    // user), unlike the generic CreatedBy audit string, which falls back to "System".
    public Guid CreatedByUserId { get; set; }

    public int? InvoiceId { get; set; }

    // The entry QR code encodes a link carrying this token (see EventEntryRules.NewToken). Each
    // phone that scans it inside the entry window takes one place, up to Capacity; AdmittedCount
    // is how many places are taken (EventAdmission holds who/when).
    public required string EntryToken { get; set; }
    public int AdmittedCount { get; set; }

    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancelReason { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public byte[] RowVersion { get; set; } = [];

    public Branch Branch { get; set; } = null!;
    public EventCustomer EventCustomer { get; set; } = null!;
    public Invoice? Invoice { get; set; }
    public ICollection<EventAdmission> Admissions { get; set; } = [];

    public decimal TotalPrice => Price + DecorationFee;
}

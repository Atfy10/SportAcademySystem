using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Events;

// One guest's entry ticket. The guest shows its QR code (a link carrying Token) at the door and
// staff scan it: a valid ticket can be admitted once, and AdmittedAt/AdmittedByUserId record who
// let it in. Tickets are issued on demand from the event page (never more than Capacity), each
// with a random 6-digit Number unique within the event (EventEntryRules.NewTicketNumber) that
// staff can type when the code won't scan. An unused one can be named, re-issued (new Token
// and new Number) or revoked; a used one is the record of who came and can't be changed.
//
// Once the event ends or is cancelled every ticket is dead for good (EventEntryRules
// .TicketsTerminated): the rows stay only as the record of who came.
public class EventTicket : ITenantScoped
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int Number { get; set; }
    // 128 random bits as 32 lowercase hex characters (EventEntryRules.NewToken), unique across
    // every academy: the public ticket page finds the ticket by it with no tenant context.
    public required string Token { get; set; }
    public string? GuestName { get; set; }
    public DateTime IssuedAt { get; set; }

    public DateTime? AdmittedAt { get; set; }
    public Guid? AdmittedByUserId { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Two doormen scanning the same ticket at once: only one save wins.
    public byte[] RowVersion { get; set; } = [];

    public Event Event { get; set; } = null!;

    public bool IsAdmitted => AdmittedAt is not null;
}

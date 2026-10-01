using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Events;

// One person let in by scanning the event's entry QR code. DeviceKey identifies the phone that
// scanned (a random id the entry page keeps in the browser), so scanning again from the same
// phone shows "already admitted" instead of taking another place. Unique per event + device.
public class EventAdmission : ITenantScoped
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public required string DeviceKey { get; set; }
    // 1 for the first person in, up to the event's capacity.
    public int Number { get; set; }
    public DateTime AdmittedAt { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Event Event { get; set; } = null!;
}

using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Events;

// Whoever rents the academy for an event. The same person books several times a year, so they
// are stored once and found again by phone number - PhoneNumber is E.164 and unique per tenant
// (among non-deleted rows), which is what makes "type the phone, get the customer" reliable.
// Tenant-scoped but deliberately NOT branch-scoped: one customer can rent at any branch, and a
// branch-restricted user still needs to find them by phone to book at their own branch.
public class EventCustomer : ITenantScoped, IAuditableEntity, ISoftDeletable
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public int NationalityCategoryId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public NationalityCategory NationalityCategory { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = [];
}

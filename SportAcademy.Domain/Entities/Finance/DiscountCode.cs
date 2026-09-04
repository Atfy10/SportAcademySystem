using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Finance;

// Percentage-off codes, minted by Owner/Admin only (see Permissions.DiscountCode.Manage) - the
// single, deliberate seam for discounting a subscription price. No usage cap, no per-branch/
// sport/trainee scoping, no fixed-amount option - not asked for. Not soft-deletable: delete is
// blocked outright while any InvoiceLine references the code (see
// IDiscountCodeRepository.HasInvoiceLinesAsync), so a hard delete is always safe when allowed;
// deactivating (IsActive = false) is how a used code is retired.
public class DiscountCode : ITenantScoped, IAuditableEntity
{
    public int Id { get; set; }
    public required string Code { get; set; }       // stored normalized (upper, trimmed) - see DiscountCodeMapper.Normalize
    public string? Description { get; set; }
    public decimal PercentageOff { get; set; }        // 0 < value <= 100
    public bool IsActive { get; set; } = true;
    public DateOnly? ExpiresAt { get; set; }           // null = no expiry, active until deactivated

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = [];
}

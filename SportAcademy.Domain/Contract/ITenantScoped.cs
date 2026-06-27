using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Contract;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
    Tenant Tenant { get; set; }
}

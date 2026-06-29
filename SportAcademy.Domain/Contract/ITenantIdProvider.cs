namespace SportAcademy.Domain.Contract;

public interface ITenantIdProvider
{
    Guid? TenantId { get; }
    void SetTenantId(Guid? tenantId);
}

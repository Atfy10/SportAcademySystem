using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Services;

public class TenantIdProvider : ITenantIdProvider
{
    private static readonly AsyncLocal<Guid?> _tenantId = new();

    public Guid? TenantId => _tenantId.Value;

    public void SetTenantId(Guid? tenantId) => _tenantId.Value = tenantId;
}

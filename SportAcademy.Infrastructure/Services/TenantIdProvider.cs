using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Services;

public class TenantIdProvider : ITenantIdProvider
{
    private static readonly AsyncLocal<Guid?> _tenantId = new();

    public Guid? TenantId => _tenantId.Value;

    public void SetTenantId(Guid? tenantId) => _tenantId.Value = tenantId;

    public IDisposable Impersonate(Guid tenantId)
    {
        var previous = _tenantId.Value;
        _tenantId.Value = tenantId;
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope : IDisposable
    {
        private readonly Guid? _previous;
        private bool _disposed;

        public RestoreScope(Guid? previous) => _previous = previous;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _tenantId.Value = _previous;
        }
    }
}

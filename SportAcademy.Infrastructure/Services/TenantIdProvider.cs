using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Services;

public class TenantIdProvider : ITenantIdProvider
{
    private static readonly AsyncLocal<Guid?> _tenantId = new();
    private static readonly AsyncLocal<bool> _allowCrossTenantWrite = new();

    public Guid? TenantId => _tenantId.Value;

    public bool AllowCrossTenantWrite => _allowCrossTenantWrite.Value;

    public void SetTenantId(Guid? tenantId) => _tenantId.Value = tenantId;

    public IDisposable Impersonate(Guid tenantId)
    {
        var previous = _tenantId.Value;
        _tenantId.Value = tenantId;
        return new RestoreScope(previous);
    }

    public IDisposable AllowCrossTenantOperation()
    {
        var previous = _allowCrossTenantWrite.Value;
        _allowCrossTenantWrite.Value = true;
        return new RestoreFlagScope(previous);
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

    private sealed class RestoreFlagScope : IDisposable
    {
        private readonly bool _previous;
        private bool _disposed;

        public RestoreFlagScope(bool previous) => _previous = previous;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _allowCrossTenantWrite.Value = _previous;
        }
    }
}

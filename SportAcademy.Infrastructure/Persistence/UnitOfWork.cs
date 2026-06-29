using Microsoft.EntityFrameworkCore.Storage;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException(
                "A transaction is already active. Commit or rollback before starting a new one.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException(
                "No active transaction to commit. Call BeginTransactionAsync first.");

        try
        {
            await _currentTransaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackInternalAsync();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException(
                "No active transaction to roll back. Call BeginTransactionAsync first.");

        try
        {
            await _currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_currentTransaction is not null)
        {
            try { await _currentTransaction.RollbackAsync(CancellationToken.None); }
            catch { /* dispose must never throw */ }
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    private async Task RollbackInternalAsync()
    {
        if (_currentTransaction is null) return;
        try { await _currentTransaction.RollbackAsync(CancellationToken.None); }
        catch { /* suppress during error recovery */ }
    }
}

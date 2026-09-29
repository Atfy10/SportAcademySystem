namespace SportAcademy.Domain.Contract;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);

    // Forgets every tracked entity. After a failed save inside a batch (e.g. one bad row of an
    // import), its half-built entities would otherwise stay tracked and be retried - and fail
    // again - on every later save in the same request.
    void ClearChangeTracker();
}

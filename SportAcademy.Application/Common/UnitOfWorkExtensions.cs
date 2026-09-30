using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Common
{
    public static class UnitOfWorkExtensions
    {
        // Runs several repository writes (each of which saves on its own) as one all-or-nothing
        // unit - the same begin/commit/rollback shape SubscriptionCreationService and
        // AcceptInvitationCommandHandler spell out by hand, for callers that don't need anything
        // between the steps.
        public static async Task<T> InTransactionAsync<T>(
            this IUnitOfWork unitOfWork, Func<Task<T>> work, CancellationToken ct = default)
        {
            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var result = await work();
                await unitOfWork.CommitTransactionAsync(ct);
                return result;
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}

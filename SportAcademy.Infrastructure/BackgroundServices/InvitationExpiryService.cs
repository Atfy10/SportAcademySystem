using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    public class InvitationExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InvitationExpiryService> _logger;

        public InvitationExpiryService(IServiceScopeFactory scopeFactory, ILogger<InvitationExpiryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Invitation expiry service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    await ExpireStaleInvitationsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during invitation expiry cleanup");
                }
            }

            _logger.LogInformation("Invitation expiry service stopped");
        }

        private async Task ExpireStaleInvitationsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantIdProvider>();

            var now = DateTime.UtcNow;

            // No ambient tenant here (outside any request) means the global query filter would
            // match zero rows across every tenant, same reasoning as EnrollmentLapseService -
            // IgnoreQueryFilters() is required to see every tenant's stale invitations at once.
            var expiredInvitations = await context.Set<Invitation>()
                .IgnoreQueryFilters()
                .Where(i => i.Status == InvitationStatus.Pending && i.ExpiresAt < now)
                .ToListAsync(ct);

            if (expiredInvitations.Count == 0)
                return;

            foreach (var invitation in expiredInvitations)
            {
                invitation.Expire();
            }

            // TenantSaveChangesInterceptor rejects a save of ITenantScoped rows with no ambient
            // tenant unless this scope explicitly acknowledges the cross-tenant batch - every
            // row above already carries its own real TenantId from the query above.
            using (tenantProvider.AllowCrossTenantOperation())
            {
                await context.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Expired {Count} stale invitations", expiredInvitations.Count);
        }
    }
}

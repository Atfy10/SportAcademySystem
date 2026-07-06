using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Options;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    public class TenantArchivalService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TenantArchivalService> _logger;

        public TenantArchivalService(IServiceScopeFactory scopeFactory, ILogger<TenantArchivalService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Tenant archival service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    await ArchiveAbandonedTenantsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during tenant archival");
                }
            }

            _logger.LogInformation("Tenant archival service stopped");
        }

        private async Task ArchiveAbandonedTenantsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var settings = scope.ServiceProvider.GetRequiredService<IOptions<TenantArchivalSettings>>();

            var threshold = DateTime.UtcNow.AddDays(-settings.Value.RetentionDays);

            var abandonedTenants = await context.Set<Tenant>()
                .Where(t => t.Status == TenantStatus.PendingSetup && t.CreatedAt < threshold)
                .ToListAsync(ct);

            if (abandonedTenants.Count == 0)
                return;

            foreach (var tenant in abandonedTenants)
            {
                tenant.Status = TenantStatus.Archived;
            }

            await context.SaveChangesAsync(ct);

            _logger.LogInformation("Archived {Count} abandoned PendingSetup tenants", abandonedTenants.Count);
        }
    }
}

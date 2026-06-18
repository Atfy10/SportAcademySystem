using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Refresh token cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    await CleanupExpiredTokensAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during refresh token cleanup");
                }
            }

            _logger.LogInformation("Refresh token cleanup service stopped");
        }

        private async Task CleanupExpiredTokensAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var threshold = DateTime.UtcNow.AddDays(-30);

            var expiredTokens = await context.RefreshTokens
                .Where(rt => rt.ExpiresAt < threshold || (rt.IsRevoked && rt.RevokedAt != null && rt.RevokedAt < threshold))
                .ExecuteDeleteAsync(ct);

            if (expiredTokens > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", expiredTokens);
            }
        }
    }
}

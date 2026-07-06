using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    public class EmailQueueCleanupService : BackgroundService
    {
        private readonly ILogger<EmailQueueCleanupService> _logger;

        public EmailQueueCleanupService(ILogger<EmailQueueCleanupService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email queue cleanup service started (stub — no email queue infrastructure yet)");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    _logger.LogDebug("Email queue cleanup tick — no-op (queue not implemented)");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during email queue cleanup");
                }
            }

            _logger.LogInformation("Email queue cleanup service stopped");
        }
    }
}

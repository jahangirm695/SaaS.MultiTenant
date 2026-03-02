using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.BackgroundServices;

public class SubscriptionExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpiryBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check every 6 hours

    public SubscriptionExpiryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionExpiryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Expiry Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Checking for expired subscriptions...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                    await subscriptionService.DeactivateExpiredSubscriptionsAsync(stoppingToken);
                }

                _logger.LogInformation("Expired subscriptions check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking expired subscriptions");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Subscription Expiry Background Service stopped");
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using kriefTrackAiApi.Infrastructure.Email;
// using kriefTrackAiApi.Infrastructure.SmsService;

namespace kriefTrackAiApi.Infrastructure.Services;

public class ShipmentNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShipmentNotificationBackgroundService> _logger;
    private readonly BackgroundTaskSync _sync;

    public ShipmentNotificationBackgroundService(IServiceProvider serviceProvider, ILogger<ShipmentNotificationBackgroundService> logger, BackgroundTaskSync sync)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _sync = sync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Shipment Notification Background Service is waiting for middleware to complete...");

        await _sync.MiddlewareCompleted.Task;

        _logger.LogInformation("Middleware completed. Starting SMS and Email notifications...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // var smsService = scope.ServiceProvider.GetRequiredService<SmsNotificationService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<EmailNotificationService>();

                    _logger.LogInformation("Sending SMS notifications...");
                    // await smsService.SendShipmentSmsAsync();

                    _logger.LogInformation("Sending Email notifications...");
                    await emailService.SendShipmentEmailsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in background service: {ex.Message}");
            }

            _logger.LogInformation("Shipment Notification Background Service sleeping...");
            //convert to send day
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}

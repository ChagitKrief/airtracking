using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using kriefTrackAiApi.Infrastructure.Middleware;

namespace kriefTrackAiApi.Infrastructure.Services;

public class DailyShipmentJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BackgroundTaskSync _sync;

    public DailyShipmentJob(IServiceProvider serviceProvider, BackgroundTaskSync sync)
    {
        _serviceProvider = serviceProvider;
        _sync = sync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var middleware = scope.ServiceProvider.GetRequiredService<WinwordDataMiddleware>();
                await middleware.FetchAndSaveSmsDataAsync();
            }
            _sync.MiddlewareCompleted.TrySetResult(true);

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}

using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Common.Dto;
using System.Net.Http.Headers;

namespace kriefTrackAiApi.Infrastructure.Services;

public class ContainerTrackingService : BackgroundService
{
    private readonly ConnectionManager _connectionManager;
    private readonly IContainerNotifier _containerNotifier;
    private readonly ILogger<ContainerTrackingService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    public ContainerTrackingService(
        ConnectionManager connectionManager,
        IContainerNotifier containerNotifier,
        ILogger<ContainerTrackingService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _connectionManager = connectionManager;
        _containerNotifier = containerNotifier;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ContainerTrackingService started.");

        using var scope = _serviceProvider.CreateScope();
        var prerenderService = scope.ServiceProvider.GetRequiredService<BlazorPrerenderService>();
        var cache = scope.ServiceProvider.GetRequiredService<PrerenderCache>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var trackedContainers = _connectionManager.GetTrackedContainers();
                _logger.LogInformation("Tracking {ContainerCount} containers.", trackedContainers.Count);

                foreach (var (containerId, userIds) in trackedContainers)
                {
                    try
                    {
                        string mapboxRoute = _configuration["Blazor:MapboxRoute"]
                            ?? throw new ArgumentNullException("Blazor:MapboxRoute is missing");

                        string url = $"{mapboxRoute}/{containerId}";
                        string html = await prerenderService.PrerenderPageAsync(url); // This triggers Windward + JS

                        cache.Save(containerId, html);
                        _logger.LogInformation("Refreshed SSR HTML for container {ContainerId}", containerId);

                        foreach (var userId in userIds)
                        {
                            await _containerNotifier.NotifyRenderedMapWithContainerAsync(userId, containerId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error rendering container {ContainerId}", containerId);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while tracking containers.");
            }

            _logger.LogInformation("Waiting 10 minutes before next tracking cycle...");
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }

        _logger.LogInformation("ContainerTrackingService is stopping.");
    }

}

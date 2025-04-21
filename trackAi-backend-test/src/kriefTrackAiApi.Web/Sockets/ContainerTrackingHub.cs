using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using kriefTrackAiApi.Infrastructure.Services;
// using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using kriefTrackAiApi.Core.Interfaces;

namespace kriefTrackAiApi.Web.Sockets;

// [AllowAnonymous]
public class ContainerTrackingHub : Hub
{
    private readonly ConnectionManager _connectionManager;
    private readonly ILogger<ContainerTrackingHub> _logger;
    private readonly IContainerNotifier _containerNotifier;
    private readonly BlazorPrerenderService _prerenderService;
    private readonly PrerenderCache _cache;
    public ContainerTrackingHub(
        ConnectionManager connectionManager,
        ILogger<ContainerTrackingHub> logger,
        IContainerNotifier containerNotifier, BlazorPrerenderService prerenderService,
    PrerenderCache cache)
    {
        _connectionManager = connectionManager;
        _logger = logger;
        _containerNotifier = containerNotifier;
        _prerenderService = prerenderService;
        _cache = cache;
    }


    public override async Task OnConnectedAsync()
    {
        string? userId = Context.GetHttpContext()?.Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            _connectionManager.AddConnection(userId, Context.ConnectionId);
            _logger.LogInformation("User {UserId} connected with ConnectionId={ConnectionId}", userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogWarning("A user connected without a userId in query parameters.");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionManager.RemoveConnection(Context.ConnectionId);
        _logger.LogInformation("Connection {ConnectionId} disconnected.", Context.ConnectionId);

        if (exception != null)
        {
            _logger.LogError(exception, "Error occurred while disconnecting ConnectionId={ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
    public Task UntrackContainer(string userId)
    {
        _connectionManager.RemoveContainerFromUser(userId);
        _logger.LogInformation("User {UserId} stopped tracking containers.", userId);
        return Task.CompletedTask;
    }
    public async Task TrackContainer(string userId, string containerId)
    {
        _connectionManager.SetUserContainer(userId, containerId);
        _logger.LogInformation("User {UserId} started tracking Container {ContainerId}.", userId, containerId);

        try
        {
            if (!_cache.TryGet(containerId, out var html))
            {
                string url = $"blazor/mapbox/{containerId}";
                _logger.LogInformation("Immediate prerender for container {ContainerId}", containerId);
                html = await _prerenderService.PrerenderPageAsync(url);
                _cache.Save(containerId, html);
                _logger.LogInformation("Stored prerendered HTML for container {ContainerId}", containerId);
            }
            else
            {
                _logger.LogInformation("Serving cached prerender for container {ContainerId}", containerId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during immediate prerender for container {ContainerId}", containerId);
        }

        await _containerNotifier.NotifyRenderedMapWithContainerAsync(userId, containerId);
    }

}

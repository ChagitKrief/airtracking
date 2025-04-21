using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Web.Sockets;

namespace kriefTrackAiApi.Web.Sockets;

public class ContainerNotifier : IContainerNotifier
{
    private readonly IHubContext<ContainerTrackingHub> _hubContext;


    public ContainerNotifier(IHubContext<ContainerTrackingHub> hubContext)
    {
        _hubContext = hubContext;

    }

    public async Task NotifyRenderedMapWithContainerAsync(string userId, string containerId)
    {
        await _hubContext.Clients.User(userId).SendAsync("RefreshMapData", containerId);
    }

}

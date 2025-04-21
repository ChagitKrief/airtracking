using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.UseCases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kriefTrackAiApi.Infrastructure.Services;

namespace kriefTrackAiApi.WebApi.Controllers;


[Route("mapbox/{containerId}")]
public class PrerenderedMapController : Controller
{
    private readonly PrerenderCache _cache;
    public PrerenderedMapController(PrerenderCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public IActionResult Get(string containerId)
    {
        if (Request.Headers.TryGetValue("X-SSR-Request", out var ssrHeader) && ssrHeader == "true")
        {
            if (_cache.TryGet(containerId, out var html))
            {
                return Content(html, "text/html");
            }
            return StatusCode(503, "SSR page not ready");
        }
        if (_cache.TryGet(containerId, out var fallbackHtml))
        {
            return Content(fallbackHtml, "text/html");
        }
        return StatusCode(202, "Page is being prerendered. Please try again shortly.");
    }

}

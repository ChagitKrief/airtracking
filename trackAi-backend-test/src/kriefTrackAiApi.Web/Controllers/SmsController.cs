using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.UseCases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace kriefTrackAiApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SmsController : ControllerBase
{
    private readonly ISmsService _service;

    public SmsController(ISmsService service)
    {
        _service = service;
    }

    [HttpPost("{container}")]
    public async Task<ActionResult<SmsDto>> AddOrUpdate(string container, [FromBody] UserPhoneEntryDto entry)
    {
        try
        {
            var result = await _service.AddOrUpdateEntryAsync(container, entry);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("Duplicate phone"))
        {
            return Conflict(new { message = ex.Message }); // 409 Conflict
        }
    }

    [HttpDelete("{container}/{userId}")]
    public async Task<ActionResult<bool>> Delete(string container, Guid userId)
    {
        var result = await _service.RemoveEntryAsync(container, userId);
        return Ok(result);
    }

    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<List<SmsDto>>> GetAllByUser(Guid userId)
    {
        var result = await _service.GetAllByUserAsync(userId);
        return Ok(result);
    }
}


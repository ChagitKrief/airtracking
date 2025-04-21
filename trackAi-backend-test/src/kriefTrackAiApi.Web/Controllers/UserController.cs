using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.UseCases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;



namespace kriefTrackAiApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    // [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    // [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Get(Guid id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    // [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Post([FromBody] UserDto userDto)
    {
        try
        {
            var createdUser = await _service.AddAsync(userDto);
            return CreatedAtAction(nameof(Get), new { id = createdUser.Id }, createdUser);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    // [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UserDto userDto)
    {
        userDto.Id = id;
        var updatedUser = await _service.UpdateAsync(userDto);
        return Ok(updatedUser);
    }

    [HttpDelete("{id}")]
    // [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}

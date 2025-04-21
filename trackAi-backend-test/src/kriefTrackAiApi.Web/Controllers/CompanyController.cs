using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.UseCases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kriefTrackAiApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _service;

    public CompanyController(ICompanyService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var companies = await _service.GetAllAsync();
        return Ok(companies);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Get(Guid id)
    {
        var company = await _service.GetByIdAsync(id);
        if (company == null) return NotFound();
        return Ok(company);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Post([FromBody] CompanyDto companyDto)
    {
        try
        {
            var createdCompany = await _service.AddAsync(companyDto);
            return CreatedAtAction(nameof(Get), new { id = createdCompany.Id }, createdCompany);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }


    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Put(Guid id, [FromBody] CompanyDto companyDto)
    {
        companyDto.Id = id;
        var updatedCompany = await _service.UpdateAsync(companyDto);
        return Ok(updatedCompany);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
    [HttpGet("with-users")]
    [Authorize(Policy = "AdminOrManager")]
    public async Task<IActionResult> GetCompaniesWithUsers()
    {
        var data = await _service.GetCompaniesWithUsersAsync();
        return Ok(data);
    }
}

using Microsoft.AspNetCore.Mvc;
using kriefTrackAiApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;

namespace kriefTrackAiApi.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WinwordController : ControllerBase
{
    private readonly WinwordFilterService _filterService;

    public WinwordController(WinwordFilterService filterService)
    {
        _filterService = filterService;
    }

    // GET: /api/winword/filter?field=shipment_delay_reasons&value=''
    [HttpGet("filter")]
    [Authorize(Policy = "AllUsers")]
    public async Task<IActionResult> GetFilteredShipments(
     [FromQuery(Name = "fields")] List<string> fields,
     [FromQuery(Name = "values")] List<string> values,
     [FromQuery(Name = "customerCodes")] List<string> customerCodes
     )
    {
        if (fields == null || values == null || fields.Count != values.Count)
            return BadRequest("fields and values must be non-empty and of equal length.");


        var result = await _filterService.FetchFilteredShipmentsAsync(fields, values, customerCodes);
        return Content(result, "application/json");
    }

}

using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Dashboard;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service) => _service = service;

    [HttpGet("summary")]
    public ActionResult<DashboardAbonosDto> GetSummary()
    {
        try { return Ok(_service.GetSummary()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

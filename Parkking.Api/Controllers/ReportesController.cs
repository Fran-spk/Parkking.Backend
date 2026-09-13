using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ReportesService _service;

    public ReportesController(ReportesService service) => _service = service;

    /// <summary>PDF con el mismo resumen que el dashboard principal.</summary>
    [HttpGet("dashboard.pdf")]
    public IActionResult DashboardPdf()
    {
        try
        {
            var (bytes, fileName) = _service.GenerarDashboardPdf();
            return File(bytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Excel de pagos filtrado por fecha de cobro (defaults: mes calendario actual).</summary>
    [HttpGet("pagos.xlsx")]
    public IActionResult PagosExcel([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        try
        {
            var (bytes, fileName) = _service.GenerarPagosExcel(desde, hasta);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Mensajeria;
using Parkking.Services;
using Parkking.Services.Mensajeria;

namespace Parkking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ReportesService _service;
    private readonly MensajeService _mensajes;

    public ReportesController(ReportesService service, MensajeService mensajes)
    {
        _service = service;
        _mensajes = mensajes;
    }

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

    [HttpPost("dashboard/enviar-email")]
    public async Task<IActionResult> EnviarDashboardEmail(
        [FromBody] EnviarReporteEmailRequest? request,
        CancellationToken ct)
    {
        try
        {
            var body = request ?? new EnviarReporteEmailRequest();
            var (bytes, fileName) = _service.GenerarDashboardPdf();
            var msg = await _mensajes.EnviarReporteAsync(
                "Reporte dashboard (PDF)",
                bytes,
                fileName,
                "application/pdf",
                body,
                ct);
            return Ok(msg);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("pagos/enviar-email")]
    public async Task<IActionResult> EnviarPagosEmail(
        [FromBody] EnviarReporteEmailRequest? request,
        CancellationToken ct)
    {
        try
        {
            var body = request ?? new EnviarReporteEmailRequest();
            var (bytes, fileName) = _service.GenerarPagosExcel(body.Desde, body.Hasta);
            var msg = await _mensajes.EnviarReporteAsync(
                "Reporte de pagos (Excel)",
                bytes,
                fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                body,
                ct);
            return Ok(msg);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

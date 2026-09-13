using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Recibos;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReciboController : ControllerBase
{
    private readonly ReciboService _service;

    public ReciboController(ReciboService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<ReciboDto>> Listar(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? cliente,
        [FromQuery] string? q)
    {
        try { return Ok(_service.Listar(desde, hasta, cliente, q)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:int}")]
    public ActionResult<ReciboDto> GetById(int id)
    {
        try { return Ok(_service.GetById(id)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpGet("pago/{pagoId:int}")]
    public ActionResult<ReciboDto> GetByPago(int pagoId)
    {
        try { return Ok(_service.GetByPagoId(pagoId)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpPost("{id:int}/anular")]
    public ActionResult<ReciboDto> Anular(int id, [FromBody] AnularReciboRequest? request)
    {
        try { return Ok(_service.Anular(id, request?.Motivo)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

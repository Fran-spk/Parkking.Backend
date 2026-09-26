using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Parkking.DTOs.Mensajeria;
using Parkking.DTOs.Recibos;
using Parkking.Services;
using Parkking.Services.Mensajeria;

namespace Parkking.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ReciboController : ControllerBase
{
    private readonly ReciboService _service;
    private readonly MensajeService _mensajes;

    public ReciboController(ReciboService service, MensajeService mensajes)
    {
        _service = service;
        _mensajes = mensajes;
    }

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

    /// <summary>Envía el recibo por email al cliente (o email del abono / override).</summary>
    [HttpPost("{id:int}/enviar-email")]
    public async Task<ActionResult<MensajeDto>> EnviarEmail(
        int id,
        [FromQuery] string? email,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] EnviarReciboEmailRequest? request,
        CancellationToken ct)
    {
        try
        {
            // Query tiene prioridad (evita líos de binding del body); body como fallback.
            var overrideEmail = !string.IsNullOrWhiteSpace(email)
                ? email
                : request?.Email;
            return Ok(await _mensajes.EnviarReciboAsync(id, overrideEmail, ct));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

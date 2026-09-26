using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Mensajeria;
using Parkking.Services.Mensajeria;

namespace Parkking.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MensajeController : ControllerBase
{
    private readonly MensajeService _mensajes;

    public MensajeController(MensajeService mensajes) => _mensajes = mensajes;

    /// <summary>Listado auditável de mensajes enviados del estacionamiento.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<MensajeDto>> Listar(
        [FromQuery] string? tipo,
        [FromQuery] string? estado,
        [FromQuery] int take = 100)
    {
        try { return Ok(_mensajes.Listar(tipo, estado, take)); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Histórico filtrado por abono (compat / consultas puntuales).</summary>
    [HttpGet("abono/{abonoId:int}")]
    public ActionResult<IEnumerable<MensajeDto>> PorAbono(int abonoId, [FromQuery] int take = 50)
    {
        try { return Ok(_mensajes.ListarPorAbono(abonoId, take)); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Histórico de envíos de un recibo concreto.</summary>
    [HttpGet("recibo/{reciboId:int}")]
    public ActionResult<IEnumerable<MensajeDto>> PorRecibo(int reciboId, [FromQuery] int take = 50)
    {
        try { return Ok(_mensajes.ListarPorRecibo(reciboId, take)); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }
}

using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Pagos;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagoMensualController : ControllerBase
{
    private readonly PagoService _service;

    public PagoMensualController(PagoService service) => _service = service;

    [HttpPost("pagar")]
    public ActionResult RegistrarPago([FromBody] RegistrarPagoRequest request)
    {
        try { _service.RegistrarPago(request); return Ok("Pago registrado exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("sugerido/{abonoId}")]
    public ActionResult GetSugerido(int abonoId, [FromQuery] DateOnly? mes)
    {
        try { return Ok(_service.GetSugerido(abonoId, mes)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("abono/{id}")]
    public ActionResult GetByAbono(int id) => Ok(_service.GetByAbono(id));

    [HttpGet("cliente/{id}")]
    public ActionResult GetByCliente(int id) => Ok(_service.GetByCliente(id));

    [HttpGet("deuda/{id}")]
    public ActionResult GetDeuda(int id)
    {
        try { return Ok(_service.GetDeuda(id)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpGet]
    public ActionResult GetAll([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta) =>
        Ok(_service.GetAll(desde, hasta));
}

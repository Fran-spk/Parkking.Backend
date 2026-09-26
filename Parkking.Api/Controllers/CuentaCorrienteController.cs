using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Finanzas;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/cuentas-corrientes")]
public class CuentaCorrienteController : ControllerBase
{
    private readonly CuentaCorrienteService _service;

    public CuentaCorrienteController(CuentaCorrienteService service) => _service = service;

    [HttpGet("estacionamiento")]
    public ActionResult<CuentaCorrienteConsultaDto> Estacionamiento(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        try { return Ok(_service.ConsultarEstacionamiento(desde, hasta)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("clientes/{clienteId:int}")]
    public ActionResult<CuentaCorrienteConsultaDto> Cliente(
        int clienteId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        try { return Ok(_service.ConsultarCliente(clienteId, desde, hasta)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

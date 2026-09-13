using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.MetodosDePago;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetodoDePagoController : ControllerBase
{
    private readonly MetodoDePagoService _service;

    public MetodoDePagoController(MetodoDePagoService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<MetodoDePagoDto>> GetAll([FromQuery] bool includeInactivos = false)
    {
        try { return Ok(_service.GetAll(includeInactivos)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:int}")]
    public ActionResult<MetodoDePagoDto> GetById(int id)
    {
        try
        {
            var m = _service.GetById(id);
            return m == null ? NotFound("Método de pago no encontrado") : Ok(m);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult<MetodoDePagoDto> Crear([FromBody] CrearMetodoDePagoRequest request)
    {
        try { return Ok(_service.Create(request.Nombre)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public ActionResult<MetodoDePagoDto> Editar(int id, [FromBody] EditarMetodoDePagoRequest request)
    {
        try { return Ok(_service.Update(id, request.Nombre)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public ActionResult DarDeBaja(int id)
    {
        try
        {
            _service.Deactivate(id);
            return Ok("Método de pago dado de baja exitosamente");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}/reactivar")]
    public ActionResult Reactivar(int id)
    {
        try
        {
            _service.Reactivate(id);
            return Ok("Método de pago reactivado exitosamente");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

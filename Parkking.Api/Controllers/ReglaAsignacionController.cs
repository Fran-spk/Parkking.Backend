using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Finanzas;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/reglas-asignacion")]
public class ReglaAsignacionController : ControllerBase
{
    private readonly ReglaAsignacionService _service;

    public ReglaAsignacionController(ReglaAsignacionService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<ReglaAsignacionDto>> GetAll([FromQuery] bool includeInactivas = false)
    {
        try { return Ok(_service.GetAll(includeInactivas)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:int}")]
    public ActionResult<ReglaAsignacionDto> GetById(int id)
    {
        try
        {
            var regla = _service.GetById(id);
            return regla == null ? NotFound("Regla de asignación no encontrada") : Ok(regla);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult<ReglaAsignacionDto> Crear([FromBody] CrearReglaAsignacionRequest request)
    {
        try { return Ok(_service.Create(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public ActionResult<ReglaAsignacionDto> Editar(int id, [FromBody] EditarReglaAsignacionRequest request)
    {
        try { return Ok(_service.Update(id, request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public ActionResult DarDeBaja(int id)
    {
        try
        {
            _service.Deactivate(id);
            return Ok("Regla de asignación dada de baja");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}/reactivar")]
    public ActionResult Reactivar(int id)
    {
        try
        {
            _service.Reactivate(id);
            return Ok("Regla de asignación reactivada");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

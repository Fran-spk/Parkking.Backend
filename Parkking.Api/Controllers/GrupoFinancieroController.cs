using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Finanzas;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/grupos-financieros")]
public class GrupoFinancieroController : ControllerBase
{
    private readonly GrupoFinancieroService _service;

    public GrupoFinancieroController(GrupoFinancieroService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<GrupoFinancieroDto>> GetAll([FromQuery] bool includeInactivos = false)
    {
        try { return Ok(_service.GetAll(includeInactivos)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:int}/periodo")]
    public ActionResult<GrupoFinancieroPeriodoDto> Periodo(
        int id,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        try { return Ok(_service.ConsultarPeriodo(id, desde, hasta)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:int}")]
    public ActionResult<GrupoFinancieroDto> GetById(int id)
    {
        try
        {
            var grupo = _service.GetById(id);
            return grupo == null ? NotFound("Grupo financiero no encontrado") : Ok(grupo);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult<GrupoFinancieroDto> Crear([FromBody] CrearGrupoFinancieroRequest request)
    {
        try { return Ok(_service.Create(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public ActionResult<GrupoFinancieroDto> Editar(int id, [FromBody] EditarGrupoFinancieroRequest request)
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
            return Ok("Grupo financiero dado de baja");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}/reactivar")]
    public ActionResult Reactivar(int id)
    {
        try
        {
            _service.Reactivate(id);
            return Ok("Grupo financiero reactivado");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

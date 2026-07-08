using Microsoft.AspNetCore.Mvc;
using Parkking.Models.Seguridad;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GrupoController : ControllerBase
{
    private readonly GrupoService _service;

    public GrupoController(GrupoService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<Grupo>> GetAllGrupos()
    {
        try { return Ok(_service.GetAll()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
    /*
    [HttpGet("byestado/{estadoId}")]
    public ActionResult<IEnumerable<Grupo>> GetAllGruposByEstado(int estadoId)
    {
        try
        {
            var estado = _service.GetAllEstados().FirstOrDefault(e => e.EST_GRU_ID == estadoId);
            if (estado == null) return NotFound("Estado de grupo no encontrado");
            return Ok(_service.GetByEstado(estado));
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }*/



    [HttpGet("modulos")]
    public ActionResult<IEnumerable<Modulo>> GetAllModulos()
    {
        try { return Ok(_service.GetAllModulos()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult AgregarGrupo([FromBody] Grupo grupo)
    {
        try { _service.Create(grupo); return Ok("Grupo agregado correctamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id}")]
    public ActionResult EliminarGrupo(int id)
    {
        try
        {
            var grupo = _service.GetById(id);
            _service.Delete(grupo!);
            return Ok("El grupo fue eliminado");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}")]
    public ActionResult ModificarGrupo(int id, [FromBody] Grupo grupo)
    {
        try { grupo.GRU_ID = id; _service.Update(grupo); return Ok("El grupo fue modificado"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

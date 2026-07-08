using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Categorias;
using Parkking.Models;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriaCocheraController : ControllerBase
{
    private readonly CategoriaService _service;

    public CategoriaCocheraController(CategoriaService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<CategoriaCochera>> GetAll([FromQuery] bool includeInactivos = false)
    {
        try { return Ok(_service.GetAll(includeInactivos)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult AgregarCategoria([FromBody] CategoriaCocheraRequest request)
    {
        try { return Ok(_service.Create(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}")]
    public ActionResult ModificarCategoria(int id, [FromBody] CategoriaCocheraRequest request)
    {
        try { return Ok(_service.Update(id, request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id}")]
    public ActionResult DarDeBaja(int id)
    {
        try { _service.Deactivate(id); return Ok("Categoría dada de baja exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}/reactivar")]
    public ActionResult Reactivar(int id)
    {
        try { _service.Reactivate(id); return Ok("Categoría reactivada exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

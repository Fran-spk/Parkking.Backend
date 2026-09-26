using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Finanzas;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/tipos-gasto")]
public class TipoGastoController : ControllerBase
{
    private readonly TipoGastoService _service;

    public TipoGastoController(TipoGastoService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<TipoGastoDto>> GetAll([FromQuery] bool includeInactivos = false)
    {
        try { return Ok(_service.GetAll(includeInactivos)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:int}")]
    public ActionResult<TipoGastoDto> GetById(int id)
    {
        try
        {
            var tipo = _service.GetById(id);
            return tipo == null ? NotFound("Tipo de gasto no encontrado") : Ok(tipo);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult<TipoGastoDto> Crear([FromBody] CrearTipoGastoRequest request)
    {
        try { return Ok(_service.Create(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public ActionResult<TipoGastoDto> Editar(int id, [FromBody] EditarTipoGastoRequest request)
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
            return Ok("Tipo de gasto dado de baja");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}/reactivar")]
    public ActionResult Reactivar(int id)
    {
        try
        {
            _service.Reactivate(id);
            return Ok("Tipo de gasto reactivado");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

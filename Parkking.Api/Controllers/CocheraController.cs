using Mapster;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Cocheras;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CocheraController : ControllerBase
{
    private readonly CocheraService _service;

    public CocheraController(CocheraService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<CocheraResponseDto>> GetAll()
    {
        try { return Ok(_service.GetAll().Adapt<IEnumerable<CocheraResponseDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("libres/{tipoVehiculoId}")]
    public ActionResult<IEnumerable<CocheraResponseDto>> GetLibresByTipoVehiculo(int tipoVehiculoId)
    {
        try { return Ok(_service.GetLibresByTipoVehiculo(tipoVehiculoId).Adapt<IEnumerable<CocheraResponseDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult<CocheraResponseDto> AgregarCochera([FromBody] CocheraRequest request)
    {
        try { return Ok(_service.Create(request).Adapt<CocheraResponseDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}")]
    public ActionResult<CocheraResponseDto> ModificarCochera(int id, [FromBody] CocheraRequest request)
    {
        try { return Ok(_service.Update(id, request).Adapt<CocheraResponseDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id}")]
    public ActionResult DesactivarCochera(int id)
    {
        try { _service.Deactivate(id); return Ok("Cochera desactivada exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

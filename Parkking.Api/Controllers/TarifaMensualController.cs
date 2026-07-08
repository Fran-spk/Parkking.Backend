using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Tarifas;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TarifaMensualController : ControllerBase
{
    private readonly TarifaMensualService _service;

    public TarifaMensualController(TarifaMensualService service) => _service = service;

    [HttpGet("vigentes")]
    public ActionResult<IEnumerable<TarifaVigenteDto>> GetVigentes()
    {
        try { return Ok(_service.GetVigentes()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("vigente")]
    public ActionResult<TarifaVigenteDto> GetVigente([FromQuery] int tipoVehiculoId, [FromQuery] int categoriaCocheraId)
    {
        try { return Ok(_service.GetVigente(tipoVehiculoId, categoriaCocheraId)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpGet("historial")]
    public ActionResult<IEnumerable<TarifaHistorialDto>> GetHistorial([FromQuery] int tipoVehiculoId, [FromQuery] int categoriaCocheraId)
    {
        try { return Ok(_service.GetHistorial(tipoVehiculoId, categoriaCocheraId)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult<TarifaVigenteDto> AgregarTarifa([FromBody] CrearTarifaRequest request)
    {
        try
        {
            var tarifa = _service.Create(request);
            return Ok(new TarifaVigenteDto
            {
                TarifaMensualId = tarifa.TarifaMensualId,
                TipoVehiculoId = tarifa.TipoVehiculoId,
                CategoriaCocheraId = tarifa.CategoriaCocheraId,
                Precio = tarifa.Precio,
                FechaActualizacion = tarifa.FechaHoraActualizacion
            });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Tarifas;
using Parkking.Models.Enums;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TarifaMensualController : ControllerBase
{
    private readonly TarifaMensualService _service;

    public TarifaMensualController(TarifaMensualService service) => _service = service;

    /// <summary>Tarifas vigentes. Opcional: filtrar por periodicidad.</summary>
    [HttpGet("vigentes")]
    public ActionResult<IEnumerable<TarifaVigenteDto>> GetVigentes([FromQuery] PeriodicidadCobro? periodicidadCobro = null)
    {
        try { return Ok(_service.GetVigentes(periodicidadCobro)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("vigente")]
    public ActionResult<TarifaVigenteDto> GetVigente(
        [FromQuery] int tipoVehiculoId,
        [FromQuery] int categoriaCocheraId,
        [FromQuery] PeriodicidadCobro periodicidadCobro = PeriodicidadCobro.Mensual)
    {
        try { return Ok(_service.GetVigente(tipoVehiculoId, categoriaCocheraId, periodicidadCobro)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpGet("historial")]
    public ActionResult<IEnumerable<TarifaHistorialDto>> GetHistorial(
        [FromQuery] int tipoVehiculoId,
        [FromQuery] int categoriaCocheraId,
        [FromQuery] PeriodicidadCobro periodicidadCobro = PeriodicidadCobro.Mensual)
    {
        try { return Ok(_service.GetHistorial(tipoVehiculoId, categoriaCocheraId, periodicidadCobro)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    public ActionResult<TarifaVigenteDto> AgregarTarifa([FromBody] CrearTarifaRequest request)
    {
        try
        {
            var tarifa = _service.Create(request);
            return Ok(TarifaMensualService.ToVigenteDto(tarifa));
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

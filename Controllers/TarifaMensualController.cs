using Microsoft.AspNetCore.Mvc;
using Parkking_backend.DTOs.Tarifas;
using Parkking_backend.Services;

[ApiController]
[Route("api/[controller]")]
public class TarifaMensualController : ControllerBase
{
    private readonly TarifaMensualService _service;

    public TarifaMensualController(TarifaMensualService service)
    {
        _service = service;
    }

    // GET /api/tarifamensual/vigentes
    [HttpGet("vigentes")]
    public ActionResult<IEnumerable<TarifaVigenteDto>> GetVigentes()
    {
        try
        {
            var tarifas = _service.GetVigentes();
            return Ok(tarifas);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET /api/tarifamensual/vigente?tipoVehiculoId=1&categoriaCocheraId=2
    [HttpGet("vigente")]
    public ActionResult<TarifaVigenteDto> GetVigente(
        [FromQuery] int tipoVehiculoId,
        [FromQuery] int categoriaCocheraId)
    {
        try
        {
            var tarifa = _service.GetVigente(tipoVehiculoId, categoriaCocheraId);
            return Ok(tarifa);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET /api/tarifamensual/historial?tipoVehiculoId=1&categoriaCocheraId=2
    [HttpGet("historial")]
    public ActionResult<IEnumerable<TarifaHistorialDto>> GetHistorial(
        [FromQuery] int tipoVehiculoId,
        [FromQuery] int categoriaCocheraId)
    {
        try
        {
            var historial = _service.GetHistorial(tipoVehiculoId, categoriaCocheraId);
            return Ok(historial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST /api/tarifamensual
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
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

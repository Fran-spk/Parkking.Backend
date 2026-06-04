using Microsoft.AspNetCore.Mvc;
using Parkking_backend.DTOs.Cocheras;
using Parkking_backend.DTOs.Shared;
using Parkking_backend.Services;

[ApiController]
[Route("api/[controller]")]
public class CocheraController : ControllerBase
{
    private readonly CocheraService _service;

    public CocheraController(CocheraService service)
    {
        _service = service;
    }

    // GET /api/cochera
    [HttpGet]
    public ActionResult<IEnumerable<CocheraResponseDto>> GetAll()
    {
        try
        {
            var cocheras = _service.GetAll();
            return Ok(cocheras.Select(c => MapToDto(c)));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET /api/cochera/libres/{tipoVehiculoId}
    [HttpGet("libres/{tipoVehiculoId}")]
    public ActionResult<IEnumerable<CocheraResponseDto>> GetLibresByTipoVehiculo(int tipoVehiculoId)
    {
        try
        {
            var cocheras = _service.GetLibresByTipoVehiculo(tipoVehiculoId);
            return Ok(cocheras.Select(c => MapToDto(c)));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST /api/cochera
    [HttpPost]
    public ActionResult<CocheraResponseDto> AgregarCochera([FromBody] CocheraRequest request)
    {
        try
        {
            var cochera = _service.Create(request);
            return Ok(MapToDto(cochera));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT /api/cochera/{id}
    [HttpPut("{id}")]
    public ActionResult<CocheraResponseDto> ModificarCochera(int id, [FromBody] CocheraRequest request)
    {
        try
        {
            var cochera = _service.Update(id, request);
            return Ok(MapToDto(cochera));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE /api/cochera/{id}
    [HttpDelete("{id}")]
    public ActionResult DesactivarCochera(int id)
    {
        try
        {
            _service.Deactivate(id);
            return Ok("Cochera desactivada exitosamente");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 🔹 Mapper
    private CocheraResponseDto MapToDto(Cochera c)
    {
        return new CocheraResponseDto
        {
            CocheraId = c.CocheraId,
            Numero = c.Numero,
            EstadoCochera = c.EstadoCochera,
            CategoriaCocheraId = c.CategoriaCocheraId,
            Observacion = c.Observacion,
            MultipleOcupacion = c.MultipleOcupacion,
            EstaDisponible = c.EstaDisponible(),
            VehiculosPermitidosIds = c.VehiculosPermitidos.Select(v => v.TipoVehiculoId).ToList(),
            CategoriaCochera = c.CategoriaCochera != null
                ? new CategoriaCocheraResumenDto
                {
                    CategoriaCocheraId = c.CategoriaCochera.CategoriaCocheraId,
                    Nombre = c.CategoriaCochera.Nombre
                }
                : null
        };
    }
}
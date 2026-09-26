using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Finanzas;
using Parkking.Infrastructure.Tenant;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/movimientos")]
public class MovimientoController : ControllerBase
{
    private readonly MovimientoService _service;
    private readonly IEstacionamientoContext _estacionamiento;

    public MovimientoController(MovimientoService service, IEstacionamientoContext estacionamiento)
    {
        _service = service;
        _estacionamiento = estacionamiento;
    }

    [HttpGet]
    public ActionResult<IEnumerable<MovimientoDto>> GetAll(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int? tipo,
        [FromQuery] int? clienteId,
        [FromQuery] int? usuarioId,
        [FromQuery] int? grupoFinancieroId)
    {
        try
        {
            return Ok(_service.GetFiltrados(
                _estacionamiento.EstacionamientoId,
                desde,
                hasta,
                tipo,
                clienteId,
                usuarioId,
                grupoFinancieroId));
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:int}")]
    public ActionResult<MovimientoDetalleDto> GetById(int id)
    {
        try
        {
            var movimiento = _service.GetById(id, _estacionamiento.EstacionamientoId);
            return movimiento == null ? NotFound("Movimiento no encontrado") : Ok(movimiento);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

using Mapster;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Caja;
using Parkking.Models.Enums;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/cajas")]
public class CajaMensualController : ControllerBase
{
    private readonly CajaMensualService _service;

    public CajaMensualController(CajaMensualService service)
    {
        _service = service;
    }

    #region Consultas



    [HttpGet("movimientos/abono/{abonoCocheraId}")]
    public IActionResult GetMovimientosByAbono(int abonoCocheraId)
    {
        try
        {
            return Ok(
                _service
                    .GetMovimientosByAbono(abonoCocheraId)
                    .Adapt<IEnumerable<MovimientoCajaDto>>()
            );
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    #endregion

    #region Reportes

    [HttpGet("resumen")]
    public IActionResult GetResumen(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        try
        {
            return Ok(
                _service
                    .GetResumen(year, month)
                    .Adapt<ResumenCajaDto>()
            );
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("movimientos/reporte")]
    public IActionResult GetReporteMovimientos(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            return Ok(
                _service
                    .GetMovimientos(year, month, null, desde, hasta)
                    .Adapt<IEnumerable<MovimientoCajaDto>>()
            );
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("conceptos")]
    public IActionResult GetConceptos(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        try
        {
            return Ok(
                _service.GetConceptos(year, month)
            );
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    #endregion

    #region Comandos

    [HttpPut("cerrar")]
    public IActionResult CerrarCaja(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        try
        {
            _service.Close(year, month);

            return Ok("Caja cerrada exitosamente");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion
}
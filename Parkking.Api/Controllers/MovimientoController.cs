using Mapster;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Caja;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/movimientos")]
public class MovimientoController : ControllerBase
{
    private readonly MovimientoService _service;

    public MovimientoController(
        MovimientoService service)
    {
        _service = service;
    }

    [HttpPost("cargo-cliente")]

    public IActionResult RegistrarCargoCliente(
        [FromBody] RegistrarMovimientoRequest request)
    {
        try
        {
            return Ok(
                _service
                    .RegistrarCargoCliente(request)
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reintegro-cliente")]
    public IActionResult RegistrarReintegroCliente(
        [FromBody] RegistrarMovimientoRequest request)
    {
        try
        {
            return Ok(
                _service
                    .RegistrarReintegroCliente(request)
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("gasto-estacionamiento")]
    public IActionResult RegistrarGastoEstacionamiento(
        [FromBody] RegistrarMovimientoRequest request)
    {
        try
        {
            return Ok(
                _service
                    .RegistrarGastoEstacionamiento(request)
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
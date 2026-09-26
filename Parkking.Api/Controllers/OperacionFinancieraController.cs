using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Finanzas;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/operaciones-financieras")]
public class OperacionFinancieraController : ControllerBase
{
    private readonly OperacionFinancieraService _service;

    public OperacionFinancieraController(OperacionFinancieraService service) => _service = service;

    [HttpGet("cargos")]
    public ActionResult<IEnumerable<CargoDto>> Cargos([FromQuery] int abonoId)
    {
        try { return Ok(_service.CargosDeAbono(abonoId)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("reintegros")]
    public ActionResult<IEnumerable<ReintegroDto>> Reintegros([FromQuery] int abonoId)
    {
        try { return Ok(_service.ReintegrosDeAbono(abonoId)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("cargos")]
    public ActionResult<OperacionFinancieraDto> CrearCargo([FromBody] CrearCargoRequest request)
    {
        try { return Ok(_service.CrearCargo(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("ajustes")]
    public ActionResult<OperacionFinancieraDto> CrearAjuste([FromBody] CrearAjusteRequest request)
    {
        try { return Ok(_service.CrearAjuste(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("reintegros")]
    public ActionResult<OperacionFinancieraDto> CrearReintegro([FromBody] CrearReintegroRequest request)
    {
        try { return Ok(_service.CrearReintegro(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("gastos")]
    public ActionResult<OperacionFinancieraDto> CrearGasto([FromBody] CrearGastoRequest request)
    {
        try { return Ok(_service.CrearGasto(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

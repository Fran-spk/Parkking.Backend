using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Abonos;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/Abono")]
[Route("api/AbonoCochera")]
public class AbonoController : ControllerBase
{
    private readonly AbonoService _service;

    public AbonoController(AbonoService service) => _service = service;

    [HttpGet("AllAbonos")]
    public ActionResult<IEnumerable<AbonoDto>> GetAllAbonos()
    {
        try { return Ok(_service.GetAll().Adapt<IEnumerable<AbonoDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("ocupadas")]
    public ActionResult<IEnumerable<AbonoDto>> GetOcupadas()
    {
        try { return Ok(_service.GetActivos().Adapt<IEnumerable<AbonoDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:int}")]
    public ActionResult<AbonoDto> GetById(int id)
    {
        try { return Ok(_service.GetById(id).Adapt<AbonoDto>()); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpGet("cliente/{clienteId}")]
    public ActionResult<IEnumerable<AbonoDto>> GetByCliente(int clienteId)
    {
        try { return Ok(_service.GetByCliente(clienteId).Adapt<IEnumerable<AbonoDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("cochera/{cocheraId}")]
    public ActionResult<IEnumerable<AbonoDto>> GetByCochera(int cocheraId)
    {
        try { return Ok(_service.GetByCochera(cocheraId).Adapt<IEnumerable<AbonoDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("patente/{patente}")]
    public ActionResult<AbonoDto> GetByPatente(string patente)
    {
        try { return Ok(_service.GetByPatente(patente).Adapt<AbonoDto>()); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    /// <summary>Búsqueda parcial por patente → lista de abonos.</summary>
    [HttpGet("buscar")]
    public ActionResult<IEnumerable<AbonoDto>> Buscar([FromQuery] string? patente)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(patente))
                return BadRequest("Indicá el parámetro patente.");

            return Ok(_service.BuscarPorPatente(patente).Adapt<IEnumerable<AbonoDto>>());
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    [HttpPost("crear")]
    public ActionResult<AbonoDto> CrearAbono([FromBody] CrearAbonoRequest request)
    {
        try { return Ok(_service.Create(request).Adapt<AbonoDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public ActionResult<AbonoDto> ModificarAbono(int id, [FromBody] ModificarAbonoRequest request)
    {
        try { return Ok(_service.Update(id, request).Adapt<AbonoDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("{id:int}/plazas")]
    public ActionResult<AbonoDto> AgregarPlaza(int id, [FromBody] AgregarPlazaRequest request)
    {
        try { return Ok(_service.AgregarPlaza(id, request).Adapt<AbonoDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}/plazas/{abonoPlazaId:int}")]
    public ActionResult<AbonoDto> RemoverPlaza(int id, int abonoPlazaId)
    {
        try { return Ok(_service.RemoverPlaza(id, abonoPlazaId).Adapt<AbonoDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}/plazas/{abonoPlazaId:int}/mover")]
    public ActionResult<AbonoDto> MoverPlaza(int id, int abonoPlazaId, [FromBody] MoverPlazaRequest request)
    {
        try { return Ok(_service.MoverPlaza(id, abonoPlazaId, request.NuevaCocheraId).Adapt<AbonoDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>Compat: mueve la primera plaza activa.</summary>
    [HttpPut("mover/{id:int}")]
    public ActionResult MoverCochera(int id, [FromBody] int nuevaCocheraId)
    {
        try { _service.MoveCochera(id, nuevaCocheraId); return Ok("Abono movido exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("{id:int}/vehiculos")]
    public ActionResult<AbonoDto> AgregarVehiculo(int id, [FromBody] AsignarVehiculoAbonoRequest request)
    {
        try { return Ok(_service.AgregarVehiculo(id, request).Adapt<AbonoDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}/vehiculos/{abonoVehiculoId:int}")]
    public ActionResult<AbonoDto> ModificarVehiculo(int id, int abonoVehiculoId, [FromBody] ModificarVehiculoAbonoRequest request)
    {
        try { return Ok(_service.ModificarVehiculo(id, abonoVehiculoId, request).Adapt<AbonoDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}/vehiculos/{abonoVehiculoId:int}")]
    public ActionResult<AbonoDto> RemoverVehiculo(int id, int abonoVehiculoId)
    {
        try { return Ok(_service.RemoverVehiculo(id, abonoVehiculoId).Adapt<AbonoDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public ActionResult DarDeBajaAbono(int id)
    {
        try { _service.Deactivate(id); return Ok("Abono dado de baja exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

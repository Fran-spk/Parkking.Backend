using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Abonos;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AbonoCocheraController : ControllerBase
{
    private readonly AbonoCocheraService _service;

    public AbonoCocheraController(AbonoCocheraService service) => _service = service;

    [HttpGet("AllAbonos")]
    public ActionResult<IEnumerable<AbonoCocheraDto>> GetAllAbonos()
    {
        try { return Ok(_service.GetAll().Adapt<IEnumerable<AbonoCocheraDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("ocupadas")]
    public ActionResult<IEnumerable<AbonoCocheraDto>> GetOcupadas()
    {
        try { return Ok(_service.GetActivos().Adapt<IEnumerable<AbonoCocheraDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("cliente/{clienteId}")]
    public ActionResult<IEnumerable<AbonoCocheraDto>> GetByCliente(int clienteId)
    {
        try { return Ok(_service.GetByCliente(clienteId).Adapt<IEnumerable<AbonoCocheraDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("cochera/{cocheraId}")]
    public ActionResult<IEnumerable<AbonoCocheraDto>> GetByCochera(int cocheraId)
    {
        try { return Ok(_service.GetByCochera(cocheraId).Adapt<IEnumerable<AbonoCocheraDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("patente/{patente}")]
    public ActionResult<AbonoCocheraDto> GetByPatente(string patente)
    {
        try { return Ok(_service.GetByPatente(patente).Adapt<AbonoCocheraDto>()); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpPost]
    [HttpPost("crear")]
    public ActionResult CrearAbono([FromBody] CrearAbonoRequest request)
    {
        try { return Ok(_service.Create(request).Adapt<AbonoCocheraDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}")]
    public ActionResult<AbonoCocheraDto> ModificarAbono(int id, [FromBody] ModificarAbonoRequest request)
    {
        try { return Ok(_service.Update(id, request).Adapt<AbonoCocheraDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("mover/{id}")]
    public ActionResult MoverCochera(int id, [FromBody] int nuevaCocheraId)
    {
        try { _service.MoveCochera(id, nuevaCocheraId); return Ok("Abono movido exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("swap")]
    public ActionResult SwapCocheras([FromBody] SwapCocherasRequest request)
    {
        try { _service.SwapCocheras(request); return Ok("Cocheras intercambiadas exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id}")]
    public ActionResult DarDeBajaAbono(int id)
    {
        try { _service.Deactivate(id); return Ok("Abono dado de baja exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

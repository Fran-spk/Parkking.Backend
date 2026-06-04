using Microsoft.AspNetCore.Mvc;
using Parkking_backend.DTOs.Abonos;
using Parkking_backend.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AbonoCocheraController : ControllerBase
{
    private readonly AbonoCocheraService _service;

    public AbonoCocheraController(AbonoCocheraService service)
    {
        _service = service;
    }
  
    [HttpGet("AllAbonos")]
    public ActionResult<IEnumerable<AbonoCocheraDto>> GetAllAbonos()
    {
        try
        {
            var abonos = _service.GetAll();
            return Ok(abonos.Adapt<IEnumerable<AbonoCocheraDto>>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("ocupadas")]
    public ActionResult<IEnumerable<AbonoCocheraDto>> GetOcupadas()
    {
        try
        {
            var abonos = _service.GetActivos();
            return Ok(abonos.Adapt<IEnumerable<AbonoCocheraDto>>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("cliente/{clienteId}")]
    public ActionResult<IEnumerable<AbonoCocheraDto>> GetByCliente(int clienteId)
    {
        try
        {
            var abonos = _service.GetByCliente(clienteId);
            return Ok(abonos.Adapt<IEnumerable<AbonoCocheraDto>>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("cochera/{cocheraId}")]
    public ActionResult<IEnumerable<AbonoCocheraDto>> GetByCochera(int cocheraId)
    {
        try
        {
            var abonos = _service.GetByCochera(cocheraId);
            return Ok(abonos.Adapt<IEnumerable<AbonoCocheraDto>>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("patente/{patente}")]
    public ActionResult<AbonoCocheraDto> GetByPatente(string patente)
    {
        try
        {
            var abono = _service.GetByPatente(patente);
            return Ok(abono.Adapt<AbonoCocheraDto>());
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [HttpPost("crear")]
    public ActionResult CrearAbono([FromBody] CrearAbonoRequest request)
    {
        try
        {
            var abono = _service.Create(request);
            return Ok(abono.Adapt<AbonoCocheraDto>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public ActionResult<AbonoCocheraDto> ModificarAbono(int id, [FromBody] ModificarAbonoRequest request)
    {
        try
        {
            var abono = _service.Update(id, request);
            return Ok(abono.Adapt<AbonoCocheraDto>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("mover/{id}")]
    public ActionResult MoverCochera(int id, [FromBody] int nuevaCocheraId)
    {
        try
        {
            _service.MoveCochera(id, nuevaCocheraId);
            return Ok("Abono movido exitosamente");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("swap")]
    public ActionResult SwapCocheras([FromBody] SwapCocherasRequest request)
    {
        try
        {
            _service.SwapCocheras(request);
            return Ok("Cocheras intercambiadas exitosamente");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public ActionResult DarDeBajaAbono(int id)
    {
        try
        {
            _service.Deactivate(id);
            return Ok("Abono dado de baja exitosamente");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
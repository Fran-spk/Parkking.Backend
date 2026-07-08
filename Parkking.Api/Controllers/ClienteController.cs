using Mapster;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Clientes;
using Parkking.Models;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _service;

    public ClienteController(ClienteService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<ClienteDto>> GetAll([FromQuery] bool includeInactivos = false)
    {
        try { return Ok(_service.GetAll(includeInactivos).Adapt<IEnumerable<ClienteDto>>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id}")]
    public ActionResult<ClienteDto> GetById(int id)
    {
         { return Ok(_service.GetById(id).Adapt<ClienteDto>()); }
       
    }

    [HttpPost]
    public ActionResult<ClienteDto> AgregarCliente([FromBody] ClienteRequest request)
    {
        try { return Ok(_service.Create(request).Adapt<ClienteDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}")]
    public ActionResult<ClienteDto> ModificarCliente(int id, [FromBody] ClienteRequest request)
    {
        try { return Ok(_service.Update(id, request).Adapt<ClienteDto>()); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id}")]
    public ActionResult DarDeBajaCliente(int id)
    {
        try { _service.Deactivate(id); return Ok("Cliente dado de baja exitosamente"); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}/reactivar")]
    public ActionResult<ClienteDto> ReactivarCliente(int id)
    {
        try
        {
            _service.Reactivate(id);
            return Ok(_service.GetById(id).Adapt<ClienteDto>());
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}

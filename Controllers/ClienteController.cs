using Microsoft.AspNetCore.Mvc;
using Parkking_backend.DTOs.Clientes;
using Parkking_backend.Models;
using Parkking_backend.Services;
using Mapster;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _service;

    public ClienteController(ClienteService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ClienteDto>> GetAll([FromQuery] bool includeInactivos = false)
    {
        try
        {
            var clientes = _service.GetAll(includeInactivos);
            return Ok(clientes.Adapt<IEnumerable<ClienteDto>>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public ActionResult<ClienteDto> GetById(int id)
    {
        try
        {
            var cliente = _service.GetById(id);
            var config = new TypeAdapterConfig();
            config.ForType<Cliente, ClienteDto>()
                  .Map(dest => dest.Abonos, src => src.Abonos.Where(a => a.Activo));
            var dto = cliente.Adapt<ClienteDto>(config);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public ActionResult<ClienteDto> AgregarCliente([FromBody] ClienteRequest request)
    {
        try
        {
            var cliente = _service.Create(request);
            return Ok(cliente.Adapt<ClienteDto>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public ActionResult<ClienteDto> ModificarCliente(int id, [FromBody] ClienteRequest request)
    {
        try
        {
            var cliente = _service.Update(id, request);
            return Ok(cliente.Adapt<ClienteDto>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public ActionResult DarDeBajaCliente(int id)
    {
        try
        {
            _service.Deactivate(id);
            return Ok("Cliente dado de baja exitosamente");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/reactivar")]
    public ActionResult<ClienteDto> ReactivarCliente(int id)
    {
        try
        {
            _service.Reactivate(id);
            var cliente = _service.GetById(id);
            return Ok(cliente.Adapt<ClienteDto>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
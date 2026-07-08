using Mapster;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Estacionamiento;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstacionamientoController : ControllerBase
{
    private readonly EstacionamientoService _service;

    public EstacionamientoController(EstacionamientoService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetActual()
    {
        return Ok(
            _service
                .GetActual()
                .Adapt<EstacionamientoDto>()
        );
    }

    [HttpPost]
    public IActionResult CrearEstacionamiento(CrearEstacionamientoRequest request)
    {
        _service.Crear(request);
        return Ok();
    }

    [HttpPut]
    public IActionResult ModificarEstacionamiento(EditarEstacionamientoRequest request)
    {
        _service.Editar(request);
        return Ok();
    }

    [HttpDelete]
    public IActionResult DesactivarEstacionamiento()
    {
        _service.BajaLogica();
        return Ok();
    }
}
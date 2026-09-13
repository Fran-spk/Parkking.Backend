using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Auth;
using Parkking.DTOs.Estacionamiento;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstacionamientoController : ControllerBase
{
    private readonly EstacionamientoService _service;
    private readonly SesionService _authService;
    public EstacionamientoController(EstacionamientoService service,SesionService sesion)
    {
        _service = service;
        _authService = sesion;
    }

    [HttpGet]
    public IActionResult GetActual()
    {
        try
        {
            return Ok(
                _service
                    .GetActual()
                    .Adapt<DatosEstacionamientoDto>()
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult CrearEstacionamiento(CrearEstacionamientoRequest request)
    {
        _service.Crear(request);
        return Ok();
    }

    [HttpPut]
    public IActionResult ModificarEstacionamiento(CrearEstacionamientoRequest request)
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

    [Authorize]
    [HttpGet("misEstacionamientos")]
    public IActionResult GetEstacionamientos()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_authService.GetEstacionamientos(usuarioId));
    }

    [Authorize]
    [HttpPost("seleccionarEstacionamiento")]
    public IActionResult SeleccionarEstacionamiento([FromBody] SeleccionarEstacionamientoRequest request)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        try
        {
            _authService.ValidarAccesoEstacionamiento(usuarioId, request.EstacionamientoId);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        Response.Cookies.Append("estacionamiento_activo", request.EstacionamientoId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddYears(1)
        });

        return Ok(new { mensaje = "Estacionamiento seleccionado" });
    }
}
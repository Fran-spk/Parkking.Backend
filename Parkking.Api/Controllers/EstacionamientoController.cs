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

    public EstacionamientoController(EstacionamientoService service, SesionService sesion)
    {
        _service = service;
        _authService = sesion;
    }

    [HttpGet]
    public ActionResult<DatosEstacionamientoDto> GetActual()
    {
        try
        {
            return Ok(_service.GetActual().Adapt<DatosEstacionamientoDto>());
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
    public ActionResult<DatosEstacionamientoDto> CrearEstacionamiento(CrearEstacionamientoRequest request)
    {
        try
        {
            return Ok(_service.Crear(request).Adapt<DatosEstacionamientoDto>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    public ActionResult<DatosEstacionamientoDto> ModificarEstacionamiento(EditarEstacionamientoRequest request)
    {
        try
        {
            return Ok(_service.Editar(request).Adapt<DatosEstacionamientoDto>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    public IActionResult DesactivarEstacionamiento()
    {
        _service.BajaLogica();
        return Ok();
    }

    [Authorize]
    [HttpGet("misEstacionamientos")]
    public ActionResult<IEnumerable<EstacionamientoUsuarioDto>> GetEstacionamientos()
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

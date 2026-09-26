using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Usuario;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _service;

    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<UsuarioDto> GetMiPerfil()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        return Ok(
            _service
                .GetMiPerfil(usuarioId)
                .Adapt<UsuarioDto>()
        );
    }

    /// <summary>Usuarios activos del estacionamiento actual (para pickers).</summary>
    [HttpGet("del-estacionamiento")]
    public ActionResult<IEnumerable<UsuarioResumenDto>> ListarDelEstacionamiento()
    {
        try
        {
            return Ok(_service.ListarDelEstacionamiento());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    public ActionResult<UsuarioDto> ModificarPerfil(EditarUsuarioRequest request)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            _service.ModificarPerfil(usuarioId, request);
            return Ok(_service.GetMiPerfil(usuarioId).Adapt<UsuarioDto>());
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

    [HttpPut("cambiarPassword")]
    public IActionResult CambiarContraseña(CambiarContraseñaRequest request)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        _service.CambiarContraseña(usuarioId, request);

        return Ok();
    }
}

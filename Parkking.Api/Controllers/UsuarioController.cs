using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Auth;
using Parkking.DTOs.Usuario;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _service;

    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetMiPerfil()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        return Ok(
            _service
                .GetMiPerfil(usuarioId)
                .Adapt<UsuarioDto>()
        );
    }

    [HttpPut]
    public IActionResult ModificarPerfil(EditarUsuarioRequest request)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        _service.ModificarPerfil(usuarioId, request);

        return Ok();
    }

    [HttpPut("cambiarPassword")]
    public IActionResult CambiarContraseña(CambiarContraseñaRequest request)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        _service.CambiarContraseña(usuarioId, request);

        return Ok();
    }
}
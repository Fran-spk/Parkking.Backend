using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Auth;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    public ActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var (usuario, token) = _authService.Login(request);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(8)
            });

            return Ok(new
            {
                mensaje = "Login exitoso",
                usuario = new
                {
                    id = usuario.USU_ID,
                    nombre = usuario.USU_USUARIO,
                    mail = usuario.USU_MAIL
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return Ok("Logout exitoso");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
            nombre = User.FindFirst(ClaimTypes.Name)?.Value,
            email = User.FindFirst(ClaimTypes.Email)?.Value
        });
    }

    [Authorize]
    [HttpGet("estacionamientos")]
    public IActionResult GetEstacionamientos()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_authService.GetEstacionamientos(usuarioId));
    }

    [Authorize]
    [HttpPost("seleccionar-estacionamiento")]
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

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
    private readonly SesionService _authService;

    public AuthController(SesionService authService) => _authService = authService;

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
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

            return Ok(new LoginResponse
            {
                Usuario = new LoginUsuarioDto
                {
                    Id = usuario.USU_ID,
                    Usuario = usuario.UsuarioName,
                    Nombre = usuario.Nombre,
                    Mail = usuario.Mail,
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
    public ActionResult<AuthMeDto> Me()
    {
        return Ok(new AuthMeDto
        {
            Id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
            Nombre = User.FindFirst(ClaimTypes.Name)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value
        });
    }
}

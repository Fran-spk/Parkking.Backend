using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MODELO.Contexto;
using MODELO.seguridad;
using Parkking_backend.Services;



namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EstacionamientoContext _context;
        private readonly JwtService _jwtService;
        private readonly UserService _userService;
        public AuthController(
            EstacionamientoContext context,
            JwtService jwtService,
            UserService userService
            )
        {
            _context = context;
            _jwtService = jwtService;
            _userService = userService;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest request)
        {
            var usuario = _context.Usuarios
                .Include(u => u.Estado_Usuario)
                .Include(u => u.Grupos)
                .ThenInclude(g => g.Estado_Grupo)
                .FirstOrDefault(u =>
                    u.USU_USUARIO == request.Login ||
                    u.USU_MAIL == request.Login);

            if (usuario == null)
                return Unauthorized("Usuario o contraseña incorrectos");

            if (!_userService.VerificarClave(
                    request.Password,
                    usuario.USU_CLAVE))
            {
                return Unauthorized("Usuario o contraseña incorrectos");
            }

            if (usuario.Estado_Usuario?.EST_USU_NOMBRE == "Inactivo")
                return Unauthorized("Usuario inactivo");

            var gruposActivos = usuario
                .getAllGruposActivos()
                .Select(g => g.GRU_NOMBRE)
                .ToList();

         /*   if (!gruposActivos.Any())
                return Unauthorized("El usuario no posee grupos activos");*/

            var token = _jwtService.GenerarToken(usuario);

            Response.Cookies.Append(
                "access_token",
                token,
                new CookieOptions
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

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");

            return Ok("Logout exitoso");
        }

    }
    public class LoginResponse
    {
        public string Token { get; set; }

        public int Id { get; set; }

        public string Usuario { get; set; }

        public string Mail { get; set; }

        public List<string> Grupos { get; set; }
    }

    public class LoginRequest
    {
        public string Login { get; set; }

        public string Password { get; set; }
    }
}
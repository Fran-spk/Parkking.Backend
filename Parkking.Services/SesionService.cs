using Parkking.DTOs.Auth;
using Parkking.DTOs.Usuario;
using Parkking.Infrastructure.Authentication;
using Parkking.Infrastructure.Security;
using Parkking.Models.Enums;
using Parkking.Models.Seguridad;
using Parkking.Repositories;

namespace Parkking.Services;

public class SesionService
{
    private readonly UsuarioRepository _repository;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtService _jwtService;

    public SesionService(UsuarioRepository repository, PasswordHasher passwordHasher, JwtService jwtService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public (Usuario usuario, string token) Login(LoginRequest request)
    {
        var usuario = _repository.GetByLogin(request.Login);
        if (usuario == null)
            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");

        if (!_passwordHasher.VerificarClave(request.Password, usuario.Clave))
            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");

        if (usuario.Estado_Usuario == EstadoUsuario.Deshabilitado)
            throw new UnauthorizedAccessException("Usuario inactivo");

        return (usuario, _jwtService.GenerarToken(usuario));
    }


    public List<EstacionamientoUsuarioDto> GetEstacionamientos(int usuarioId) =>
        _repository.GetEstacionamientosUsuario(usuarioId)
            .Select(e => new EstacionamientoUsuarioDto
            {
                Id = e.EstacionamientoId,
                Nombre = e.Nombre,
                Direccion = e.Direccion
            }).ToList();

    public void ValidarAccesoEstacionamiento(int usuarioId, int estacionamientoId)
    {
        if (!_repository.TieneAccesoEstacionamiento(usuarioId, estacionamientoId))
            throw new UnauthorizedAccessException("Sin acceso al estacionamiento");
    }

}

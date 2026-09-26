using Parkking.DTOs.Usuario;
using Parkking.Infrastructure.Security;
using Parkking.Infrastructure.Tenant;
using Parkking.Models.Seguridad;
using Parkking.Repositories;

namespace Parkking.Services;

public class UsuarioService
{
    private readonly UsuarioRepository _repository;
    private readonly PasswordHasher _passwordHasher;
    private readonly IEstacionamientoContext _tenant;

    public UsuarioService(
        UsuarioRepository repository,
        PasswordHasher passwordHasher,
        IEstacionamientoContext tenant)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tenant = tenant;
    }

    public Usuario GetMiPerfil(int usuarioId)
    {
        return _repository.GetById(usuarioId)
            ?? throw new Exception("Usuario no encontrado.");
    }

    public List<UsuarioResumenDto> ListarDelEstacionamiento()
    {
        return _repository.GetActivosPorEstacionamiento(_tenant.EstacionamientoId)
            .Select(u => new UsuarioResumenDto
            {
                UsuarioId = u.USU_ID,
                UsuarioName = u.UsuarioName,
                Nombre = u.Nombre,
                Mail = u.Mail,
            })
            .ToList();
    }

    public void ModificarPerfil(int usuarioId, EditarUsuarioRequest request)
    {
        var usuario = _repository.GetById(usuarioId)
            ?? throw new Exception("Usuario no encontrado.");

        if (usuario.Estado_Usuario == Models.Enums.EstadoUsuario.Deshabilitado)
            throw new UnauthorizedAccessException("El usuario se encuentra deshabilitado.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new Exception("Debe ingresar un nombre.");

        if (string.IsNullOrWhiteSpace(request.Mail))
            throw new Exception("Debe ingresar el email del usuario.");

        var mail = request.Mail.Trim();
        if (mail.Length > 60)
            throw new Exception("El email no puede superar 60 caracteres.");

        usuario.Nombre = request.Nombre.Trim();
        usuario.Mail = mail;
        usuario.Telefono = request.Telefono ?? string.Empty;

        _repository.SaveChanges(usuario);
    }

    public void CambiarContraseña(int usuarioId, CambiarContraseñaRequest request)
    {
        var usuario = _repository.GetById(usuarioId)
            ?? throw new Exception("Usuario no encontrado.");

        if (usuario.Estado_Usuario == Models.Enums.EstadoUsuario.Deshabilitado)
            throw new UnauthorizedAccessException("El usuario se encuentra deshabilitado.");

        if (!_passwordHasher.VerificarClave(request.passwordActual, usuario.Clave))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        usuario.Clave = _passwordHasher.EncriptarClave(request.passwordNueva);

        _repository.SaveChanges(usuario);
    }
}

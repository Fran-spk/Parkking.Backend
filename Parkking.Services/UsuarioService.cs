using Parkking.DTOs.Usuario;
using Parkking.Infrastructure.Security;
using Parkking.Models.Seguridad;
using Parkking.Repositories;

namespace Parkking.Services;

public class UsuarioService
{
    private readonly UsuarioRepository _repository;
    private readonly PasswordHasher _passwordHasher;

    public UsuarioService(
        UsuarioRepository repository,
        PasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public Usuario GetMiPerfil(int usuarioId)
    {
        return _repository.GetById(usuarioId)
            ?? throw new Exception("Usuario no encontrado.");
    }

    public void ModificarPerfil(int usuarioId, EditarUsuarioRequest request)
    {
        var usuario = _repository.GetById(usuarioId)
            ?? throw new Exception("Usuario no encontrado.");

        if (usuario.Estado_Usuario == Models.Enums.EstadoUsuario.Deshabilitado)
            throw new UnauthorizedAccessException("El usuario se encuentra deshabilitado.");

        usuario.Nombre = request.Nombre;
        usuario.Telefono = request.Telefono;

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
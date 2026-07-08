using Parkking.Models.Enums;
using Parkking.Models.Seguridad;

namespace Parkking.Infrastructure.Security;

public class PasswordHasher
{
    public string EncriptarClave(string clave)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(clave);
        var hash = sha256.ComputeHash(bytes);
        var builder = new System.Text.StringBuilder();
        foreach (var b in hash)
            builder.Append(b.ToString("x2"));
        return builder.ToString();
    }

    public bool VerificarClave(string claveIngresada, string claveGuardada)
    {
        return EncriptarClave(claveIngresada) == claveGuardada;
    }

    public string GenerarClaveTemporal(int longitud = 8)
    {
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(caracteres, longitud).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public bool UsuarioActivo(Usuario usuario)
    {
        return usuario.Estado_Usuario != null &&
               usuario.Estado_Usuario == EstadoUsuario.Deshabilitado;
    }
}

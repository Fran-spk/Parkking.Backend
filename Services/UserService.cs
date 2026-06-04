using System.Security.Cryptography;
using System.Text;
using MODELO.seguridad;

namespace Parkking_backend.Services
{
    public class UserService
    {
        public string EncriptarClave(string clave)
        {
            using SHA256 sha256 = SHA256.Create();

            byte[] bytes = Encoding.UTF8.GetBytes(clave);

            byte[] hash = sha256.ComputeHash(bytes);

            StringBuilder builder = new();

            foreach (byte b in hash)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        public bool VerificarClave(
            string claveIngresada,
            string claveGuardada)
        {
            string hashIngresado =
                EncriptarClave(claveIngresada);

            return hashIngresado == claveGuardada;
        }

        public string GenerarClaveTemporal(int longitud = 8)
        {
            const string caracteres =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var random = new Random();

            return new string(
                Enumerable.Repeat(caracteres, longitud)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray()
            );
        }

        public bool UsuarioActivo(Usuario usuario)
        {
            return usuario.Estado_Usuario != null &&
                   usuario.Estado_Usuario.EST_USU_NOMBRE != "Inactivo";
        }

        public bool TieneGruposActivos(Usuario usuario)
        {
            return usuario.getAllGruposActivos().Any();
        }
    }
}
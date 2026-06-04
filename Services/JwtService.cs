
using Microsoft.IdentityModel.Tokens;
using MODELO.seguridad;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Parkking_backend.Services
{
    public class JwtService
    {
        private readonly string _key = "MiClaveSuperSecretaDeAlMenos32Caracteres";

        public string GenerarToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    usuario.USU_CLAVE.ToString()),

                new Claim(ClaimTypes.Name,
                    usuario.USU_USUARIO),

                new Claim(ClaimTypes.Email,
                    usuario.USU_MAIL)
            };

            foreach (var grupo in usuario.Grupos)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        grupo.GRU_NOMBRE
                    )
                );
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_key)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}


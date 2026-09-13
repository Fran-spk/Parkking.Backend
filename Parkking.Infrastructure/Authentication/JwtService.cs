using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Parkking.Models.Seguridad;

namespace Parkking.Infrastructure.Authentication;

public class JwtService
{
    private readonly string _key = "MiClaveSuperSecretaDeAlMenos32Caracteres";

    public string GenerarToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.USU_ID.ToString()),
            new(ClaimTypes.Name, usuario.UsuarioName),
            new(ClaimTypes.Email, usuario.Mail)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

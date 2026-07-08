using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Parkking.DTOs.Auth;

namespace Parkking.Infrastructure.Tenant;

public class TenantProvider : IEstacionamientoContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public EstacionamientoUsuarioDto GetUsuarioActual()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var idStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        var nombre = user?.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
        var email = user?.FindFirst(ClaimTypes.Email)?.Value ?? "sistema@parkking.com";

        return new EstacionamientoUsuarioDto
        {
            Id = int.Parse(idStr),
            Nombre = nombre,
            Direccion = email 
        };
    }
    public int EstacionamientoId
    {
        get
        {
            // Validación de seguridad por si se llama fuera de una petición HTTP activa
            if (_httpContextAccessor.HttpContext == null)
                return 0;

            var cookie = _httpContextAccessor.HttpContext.Request.Cookies["estacionamiento_activo"];

            if (string.IsNullOrWhiteSpace(cookie))
                throw new UnauthorizedAccessException("No hay estacionamiento seleccionado.");

            if (!int.TryParse(cookie, out var estacionamientoId))
                throw new UnauthorizedAccessException("La cookie de estacionamiento es inválida.");

            return estacionamientoId;
        }
    }
}

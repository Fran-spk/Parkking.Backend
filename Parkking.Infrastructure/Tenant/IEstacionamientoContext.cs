using Parkking.DTOs.Auth;

namespace Parkking.Infrastructure.Tenant;

public interface IEstacionamientoContext
{
    int EstacionamientoId { get; }
    EstacionamientoUsuarioDto GetUsuarioActual();
}

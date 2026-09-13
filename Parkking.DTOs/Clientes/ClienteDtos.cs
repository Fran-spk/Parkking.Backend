using Parkking.DTOs.Abonos;

namespace Parkking.DTOs.Clientes;

public class ClienteRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Observacion { get; set; }
}

public class ClienteDto
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Observacion { get; set; }
    public bool Activo { get; set; }
    public List<AbonoDto>? Abonos { get; set; }
}

public class VehiculoDto
{
    public int VehiculoId { get; set; }
    public int ClienteId { get; set; }
    public string Patente { get; set; } = string.Empty;
    public string? ModeloVehiculo { get; set; }
    public int TipoVehiculoId { get; set; }
    public string? TipoVehiculoNombre { get; set; }
    public bool Activo { get; set; }
    /// <summary>True si ya está vinculado a un abono activo.</summary>
    public bool AsignadoAAbonoActivo { get; set; }
}

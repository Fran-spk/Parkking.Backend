using Parkking.DTOs.Shared;

namespace Parkking.DTOs.Abonos;

public class CrearAbonoRequest
{
    public int ClienteId { get; set; }
    public int CocheraId { get; set; }
    public int TipoVehiculoId { get; set; }
    public string? Patente { get; set; }
    public string? ModeloVehiculo { get; set; }
    public string? Cobrador { get; set; }
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaInicioCobro { get; set; }
    public decimal? PrecioAcordado { get; set; }
}

public class ModificarAbonoRequest
{
    public string? Patente { get; set; }
    public string? ModeloVehiculo { get; set; }
    public string? Cobrador { get; set; }
    public decimal? PrecioAcordado { get; set; }
}

public class SwapCocherasRequest
{
    public int AbonoCocheraId1 { get; set; }
    public int AbonoCocheraId2 { get; set; }
}

public class AbonoCocheraDto
{
    public int AbonoCocheraId { get; set; }
    public int ClienteId { get; set; }
    public int CocheraId { get; set; }
    public int TipoVehiculoId { get; set; }
    public string? Patente { get; set; }
    public string? ModeloVehiculo { get; set; }
    public string? Cobrador { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaInicioCobro { get; set; }
    public decimal? PrecioAcordado { get; set; }
    public bool Activo { get; set; }
    public ClienteResumenDto? Cliente { get; set; }
    public CocheraResumenDto? Cochera { get; set; }
    public TipoVehiculoResumenDto? TipoVehiculo { get; set; }
}

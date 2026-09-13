using Parkking.DTOs.Shared;
using Parkking.Models.Enums;

namespace Parkking.DTOs.Abonos;

public class CrearAbonoRequest
{
    public int ClienteId { get; set; }

    /// <summary>Plazas del abono (al menos una). Si está vacío, se usa <see cref="CocheraId"/>.</summary>
    public List<int> CocheraIds { get; set; } = new();

    /// <summary>Compat: una sola cochera cuando no se envía CocheraIds.</summary>
    public int? CocheraId { get; set; }

    public List<AsignarVehiculoAbonoRequest> Vehiculos { get; set; } = new();

    /// <summary>Compat: un vehículo fijo si Vehiculos está vacío y hay Patente.</summary>
    public int? TipoVehiculoId { get; set; }
    public string? Patente { get; set; }
    public string? ModeloVehiculo { get; set; }

    public string? Cobrador { get; set; }
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaInicioCobro { get; set; }
    public decimal? PrecioAcordado { get; set; }
    public PeriodicidadCobro PeriodicidadCobro { get; set; } = PeriodicidadCobro.Mensual;
}

public class ModificarAbonoRequest
{
    public string? Cobrador { get; set; }
    public decimal? PrecioAcordado { get; set; }
}

public class AsignarVehiculoAbonoRequest
{
    /// <summary>Vehículo existente del cliente. Si es null, se crea uno nuevo con Patente/Tipo.</summary>
    public int? VehiculoId { get; set; }

    public string? Patente { get; set; }
    public string? ModeloVehiculo { get; set; }
    public int? TipoVehiculoId { get; set; }

    public ModalidadVehiculoAbono Modalidad { get; set; } = ModalidadVehiculoAbono.Fijo;

    /// <summary>Obligatorio si Modalidad = Fijo: cochera (plaza) del abono a la que se fija.</summary>
    public int? CocheraId { get; set; }

    /// <summary>Alternativa a CocheraId: plaza ya existente del abono.</summary>
    public int? AbonoPlazaId { get; set; }
}

public class ModificarVehiculoAbonoRequest
{
    public ModalidadVehiculoAbono? Modalidad { get; set; }
    public int? AbonoPlazaId { get; set; }
    public int? CocheraId { get; set; }
    public string? Patente { get; set; }
    public string? ModeloVehiculo { get; set; }
    public int? TipoVehiculoId { get; set; }
}

public class AgregarPlazaRequest
{
    public int CocheraId { get; set; }
}

public class MoverPlazaRequest
{
    public int NuevaCocheraId { get; set; }
}

public class AbonoPlazaDto
{
    public int AbonoPlazaId { get; set; }
    public int AbonoId { get; set; }
    public int CocheraId { get; set; }
    public bool Activo { get; set; }
    public CocheraResumenDto? Cochera { get; set; }
}

public class AbonoVehiculoDto
{
    public int AbonoVehiculoId { get; set; }
    public int AbonoId { get; set; }
    public int VehiculoId { get; set; }
    public ModalidadVehiculoAbono Modalidad { get; set; }
    public int? AbonoPlazaId { get; set; }
    public string Patente { get; set; } = string.Empty;
    public string? ModeloVehiculo { get; set; }
    public int TipoVehiculoId { get; set; }
    public TipoVehiculoResumenDto? TipoVehiculo { get; set; }
    public CocheraResumenDto? Plaza { get; set; }
}

public class AbonoDto
{
    public int AbonoId { get; set; }

    /// <summary>Alias de compatibilidad JSON (= AbonoId).</summary>
    public int AbonoCocheraId { get => AbonoId; set => AbonoId = value; }

    public int ClienteId { get; set; }
    public string? Cobrador { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaInicioCobro { get; set; }
    public decimal? PrecioAcordado { get; set; }
    public PeriodicidadCobro PeriodicidadCobro { get; set; } = PeriodicidadCobro.Mensual;
    public bool Activo { get; set; }

    public ClienteResumenDto? Cliente { get; set; }
    public List<AbonoPlazaDto> Plazas { get; set; } = new();
    public List<AbonoVehiculoDto> Vehiculos { get; set; } = new();

    /// <summary>Compat: primera plaza activa.</summary>
    public int? CocheraId { get; set; }
    public CocheraResumenDto? Cochera { get; set; }

    /// <summary>Compat: primer vehículo.</summary>
    public string? Patente { get; set; }
    public string? ModeloVehiculo { get; set; }
    public int? TipoVehiculoId { get; set; }
    public TipoVehiculoResumenDto? TipoVehiculo { get; set; }
}

/// <summary>Alias legacy del DTO de respuesta.</summary>
public class AbonoCocheraDto : AbonoDto
{
}

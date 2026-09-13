using Parkking.Models.Enums;

namespace Parkking.DTOs.Tarifas;

public class AgregarTarifaRequest
{
    public int TipoVehiculoId { get; set; }
    public int CategoriaCocheraId { get; set; }
    public PeriodicidadCobro PeriodicidadCobro { get; set; } = PeriodicidadCobro.Mensual;
    public decimal Precio { get; set; }
}

public class CrearTarifaRequest
{
    public int TipoVehiculoId { get; set; }
    public int CategoriaCocheraId { get; set; }
    public PeriodicidadCobro PeriodicidadCobro { get; set; } = PeriodicidadCobro.Mensual;
    public decimal Precio { get; set; }
}

public class TarifaVigenteDto
{
    public int TarifaMensualId { get; set; }
    public int TipoVehiculoId { get; set; }
    public int CategoriaCocheraId { get; set; }
    public PeriodicidadCobro PeriodicidadCobro { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class TarifaHistorialDto
{
    public int TarifaMensualId { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

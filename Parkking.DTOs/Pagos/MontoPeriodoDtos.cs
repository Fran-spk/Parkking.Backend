using Parkking.Models.Enums;

namespace Parkking.DTOs.Pagos;

/// <summary>Línea de tarifa de lista usada para armar el monto del período.</summary>
public class ComponenteTarifaDto
{
    public int? TarifaMensualId { get; set; }
    public int VehiculoId { get; set; }
    public string Patente { get; set; } = string.Empty;
    public int TipoVehiculoId { get; set; }
    public string? TipoVehiculoNombre { get; set; }
    public int CocheraId { get; set; }
    public string? CocheraNumero { get; set; }
    public int CategoriaCocheraId { get; set; }
    public string? CategoriaNombre { get; set; }
    public PeriodicidadCobro PeriodicidadCobro { get; set; }
    public decimal Precio { get; set; }
}

/// <summary>Resultado del cálculo de monto base de un período del abono.</summary>
public class MontoPeriodoDto
{
    public int AbonoId { get; set; }
    public decimal Monto { get; set; }
    public bool EsPrecioAcordado { get; set; }
    public List<ComponenteTarifaDto> ComponentesTarifa { get; set; } = new();
}

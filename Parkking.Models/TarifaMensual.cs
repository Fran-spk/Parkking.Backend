using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models;

/// <summary>
/// Precio de lista por combinación tipo vehículo + categoría de cochera + periodicidad de cobro.
/// El historial se conserva insertando nuevas filas (vigente = última FechaHoraActualizacion).
/// </summary>
public class TarifaMensual : IMultiTenant
{
    [Key]
    public int TarifaMensualId { get; set; }

    [Required]
    [ForeignKey(nameof(Estacionamiento))]
    public int EstacionamientoId { get; set; }
    public DatosEstacionamiento Estacionamiento { get; set; } = null!;

    [ForeignKey(nameof(TipoVehiculo))]
    public int TipoVehiculoId { get; set; }
    public TipoVehiculo TipoVehiculo { get; set; } = null!;

    [ForeignKey(nameof(CategoriaCochera))]
    public int CategoriaCocheraId { get; set; }
    public CategoriaCochera CategoriaCochera { get; set; } = null!;

    /// <summary>Periodicidad a la que aplica este precio (mensual, quincenal, etc.).</summary>
    public PeriodicidadCobro PeriodicidadCobro { get; set; } = PeriodicidadCobro.Mensual;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    [Required]
    public DateTime FechaHoraActualizacion { get; set; }
}

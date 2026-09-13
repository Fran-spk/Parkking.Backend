using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models;

/// <summary>
/// Línea de liquidación de una cuota (invoice line item).
/// Congela el armado del monto al momento de crear la cuota.
/// </summary>
public class DetalleCuota : IMultiTenant
{
    [Key]
    public int DetalleCuotaId { get; set; }

    [ForeignKey(nameof(Estacionamiento))]
    public int EstacionamientoId { get; set; }
    public DatosEstacionamiento? Estacionamiento { get; set; }

    [ForeignKey(nameof(Cuota))]
    public int CuotaId { get; set; }
    public Cuota Cuota { get; set; } = null!;

    /// <summary>
    /// Tarifa de lista usada en esta línea.
    /// Null si es precio acordado, prorrateo, recargo u otro ajuste sin catálogo.
    /// </summary>
    [ForeignKey(nameof(Tarifa))]
    public int? TarifaMensualId { get; set; }
    public TarifaMensual? Tarifa { get; set; }

    public TipoDetalleCuota Tipo { get; set; } = TipoDetalleCuota.TarifaLista;

    [ForeignKey(nameof(Vehiculo))]
    public int? VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }

    [ForeignKey(nameof(Cochera))]
    public int? CocheraId { get; set; }
    public Cochera? Cochera { get; set; }

    // Snapshots: el documento no depende del catálogo vivo

    [MaxLength(10)]
    public string? Patente { get; set; }

    [MaxLength(50)]
    public string? CocheraNumero { get; set; }

    [MaxLength(80)]
    public string? TipoVehiculoNombre { get; set; }

    [MaxLength(80)]
    public string? CategoriaNombre { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Cantidad { get; set; } = 1;

    /// <summary>PrecioUnitario × Cantidad (suma al Monto de la cuota).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Importe { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

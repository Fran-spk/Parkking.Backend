using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models;

/// <summary>
/// Cobro / dinero recibido. Puede cubrir una o varias cuotas vía DetallePago.
/// </summary>
public class Pago : IMultiTenant
{
    [Key]
    public int PagoId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Abono))]
    public int AbonoId { get; set; }
    public Abono Abono { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    /// <summary>Parte del cobro correspondiente a recargo (informativo).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Recargo { get; set; }

    [Required]
    public DateTime FechaHora { get; set; }

    [MaxLength(255)]
    public string? Observacion { get; set; }

    public string? MercadoPagoId { get; set; }

    /// <summary>Método usado al cobrar. Nullable por pagos históricos previos al feature.</summary>
    [ForeignKey(nameof(MetodoDePago))]
    public int? MetodoDePagoId { get; set; }
    public MetodoDePago? MetodoDePago { get; set; }

    public ICollection<DetallePago> Detalles { get; set; } = new List<DetallePago>();
    public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();

    /// <summary>Comprobante generado al cobrar (1:1).</summary>
    public Recibo? Recibo { get; set; }
}

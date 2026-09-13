using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models;

/// <summary>
/// Comprobante inmutable de un cobro (1:1 con <see cref="Pago"/>).
/// Guarda snapshot para reimpresión aunque cambie el abono después.
/// </summary>
[Table("Recibos")]
public class Recibo : IMultiTenant
{
    [Key]
    public int ReciboId { get; set; }

    public int EstacionamientoId { get; set; }

    /// <summary>Número correlativo por estacionamiento (para mostrar: 2026-000123).</summary>
    public int Numero { get; set; }

    [ForeignKey(nameof(Pago))]
    public int PagoId { get; set; }
    public Pago Pago { get; set; } = null!;

    public int AbonoId { get; set; }

    public DateTime FechaEmision { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Recargo { get; set; }

    [Required]
    [MaxLength(200)]
    public string ClienteNombre { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CocherasLabel { get; set; }

    [MaxLength(200)]
    public string? PatentesLabel { get; set; }

    [MaxLength(500)]
    public string? PeriodosLabel { get; set; }

    [MaxLength(100)]
    public string? Cobrador { get; set; }

    /// <summary>Snapshot del método de pago al emitir (sobrevive a bajas).</summary>
    [MaxLength(100)]
    public string? MetodoPagoLabel { get; set; }

    [MaxLength(255)]
    public string? Observacion { get; set; }

    public bool Anulado { get; set; }

    [MaxLength(255)]
    public string? MotivoAnulacion { get; set; }

    [NotMapped]
    public string NumeroFormateado =>
        $"{FechaEmision.Year}-{Numero.ToString().PadLeft(6, '0')}";

    [NotMapped]
    public decimal Total => Monto + Recargo;
}

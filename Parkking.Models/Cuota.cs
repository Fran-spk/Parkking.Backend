using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models;

/// <summary>
/// Obligación / comprobante a pagar de un período del abono.
/// </summary>
public class Cuota : IMultiTenant
{
    [Key]
    public int CuotaId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Abono))]
    public int AbonoId { get; set; }
    public Abono Abono { get; set; } = null!;

    [Required]
    public DateOnly PeriodoInicio { get; set; }

    [Required]
    public DateOnly PeriodoFin { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Required]
    public EstadoCuota Estado { get; set; } = EstadoCuota.Pendiente;

    public ICollection<DetallePago> DetallesPago { get; set; } = new List<DetallePago>();

    /// <summary>Líneas de liquidación (cómo se armó el Monto).</summary>
    public ICollection<DetalleCuota> Detalles { get; set; } = new List<DetalleCuota>();

    [NotMapped]
    public decimal MontoPagado => DetallesPago?.Sum(d => d.Monto) ?? 0;

    [NotMapped]
    public decimal Saldo => Math.Max(0, Monto - MontoPagado);

    public void RecalcularEstado()
    {
        if (Estado == EstadoCuota.Anulada)
            return;

        var pagado = MontoPagado;
        if (pagado <= 0)
            Estado = EstadoCuota.Pendiente;
        else if (pagado + 0.009m < Monto) // tolerancia centavos
            Estado = EstadoCuota.Parcial;
        else
            Estado = EstadoCuota.Pagada;
    }

    public void RecalcularEstado(decimal montoPagado)
    {
        if (Estado == EstadoCuota.Anulada)
            return;

        if (montoPagado <= 0)
            Estado = EstadoCuota.Pendiente;
        else if (montoPagado + 0.009m < Monto)
            Estado = EstadoCuota.Parcial;
        else
            Estado = EstadoCuota.Pagada;
    }
}

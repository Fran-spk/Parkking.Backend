using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models;

/// <summary>
/// Desglose de un pago: cuánto se imputó a cada cuota.
/// </summary>
public class DetallePago : IMultiTenant
{
    [Key]
    public int DetallePagoId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Pago))]
    public int PagoId { get; set; }
    public Pago Pago { get; set; } = null!;

    [ForeignKey(nameof(Cuota))]
    public int CuotaId { get; set; }
    public Cuota Cuota { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models.Finanzas;

public class Gasto : IMultiTenant
{
    [Key]
    public int GastoId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(TipoGasto))]
    public int TipoGastoId { get; set; }
    public TipoGasto TipoGasto { get; set; } = null!;

    public int? ClienteId { get; set; }

    [ForeignKey(nameof(Movimiento))]
    public int MovimientoId { get; set; }
    public Movimiento Movimiento { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Importe { get; set; }

    [MaxLength(255)]
    public string? Observacion { get; set; }

    public DateTime FechaHora { get; set; }
}

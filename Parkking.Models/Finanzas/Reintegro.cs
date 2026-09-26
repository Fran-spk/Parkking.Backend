using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models.Finanzas;

public class Reintegro : IMultiTenant
{
    [Key]
    public int ReintegroId { get; set; }

    public int EstacionamientoId { get; set; }

    public int? ClienteId { get; set; }

    [ForeignKey(nameof(Movimiento))]
    public int MovimientoId { get; set; }
    public Movimiento Movimiento { get; set; } = null!;

    [Required]
    [MaxLength(120)]
    public string Beneficiario { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Importe { get; set; }

    [Required]
    [MaxLength(255)]
    public string Motivo { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Medio { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }
}

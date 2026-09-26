using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models.Finanzas;

/// <summary>Obligación. No genera movimiento: el movimiento nace cuando se paga.</summary>
public class Cargo : IMultiTenant
{
    [Key]
    public int CargoId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Cliente))]
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int? AbonoId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Importe { get; set; }

    [Required]
    [MaxLength(255)]
    public string Concepto { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }
}

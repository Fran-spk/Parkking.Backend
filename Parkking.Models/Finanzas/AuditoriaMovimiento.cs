using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models.Finanzas;

public class AuditoriaMovimiento : IMultiTenant
{
    [Key]
    public int AuditoriaMovimientoId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Movimiento))]
    public int MovimientoId { get; set; }
    public Movimiento Movimiento { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public int UsuarioId { get; set; }

    [MaxLength(45)]
    public string? Ip { get; set; }

    [MaxLength(400)]
    public string? UserAgent { get; set; }

    [Required]
    [MaxLength(500)]
    public string Detalle { get; set; } = string.Empty;
}

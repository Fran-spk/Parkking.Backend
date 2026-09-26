using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models;

/// <summary>
/// Log auditable de un email enviado (o intentado) por el sistema.
/// No es una feature del abono: vive a nivel estacionamiento.
/// Tipo: Recibo | Reporte | ActualizacionTarifas.
/// Estado: Enviado | Error | Simulado.
/// </summary>
[Table("Mensajes")]
public class Mensaje : IMultiTenant
{
    [Key]
    public int MensajeId { get; set; }

    public int EstacionamientoId { get; set; }

    /// <summary>Ej: Recibo.</summary>
    [Required]
    [MaxLength(40)]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Destinatario { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Remitente { get; set; }

    [Required]
    [MaxLength(300)]
    public string Asunto { get; set; } = string.Empty;

    /// <summary>Enviado | Error | Simulado</summary>
    [Required]
    [MaxLength(30)]
    public string Estado { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Error { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public int? ReciboId { get; set; }
    public int? ClienteId { get; set; }
    public int? AbonoId { get; set; }
}

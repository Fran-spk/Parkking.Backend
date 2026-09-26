using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models;

/// <summary>
/// Archivo adjunto anclado a exactamente un dueño: cliente, vehículo o abono.
/// La UI del abono agrega docs del abono + de sus vehículos (+ cliente).
/// </summary>
[Table("Documentos")]
public class Documento : IMultiTenant
{
    [Key]
    public int DocumentoId { get; set; }

    public int EstacionamientoId { get; set; }

    /// <summary>Ver <see cref="TipoDocumento"/>.</summary>
    [Required]
    [MaxLength(40)]
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Nombre original del archivo subido.</summary>
    [Required]
    [MaxLength(260)]
    public string NombreOriginal { get; set; } = string.Empty;

    /// <summary>Ruta relativa bajo el root de uploads (tenant/…).</summary>
    [Required]
    [MaxLength(500)]
    public string RutaRelativa { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string ContentType { get; set; } = "application/octet-stream";

    public long TamanoBytes { get; set; }

    /// <summary>Útil para pólizas de seguro.</summary>
    public DateOnly? FechaVencimiento { get; set; }

    [MaxLength(300)]
    public string? Observacion { get; set; }

    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;

    public bool Activo { get; set; } = true;

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }

    public int? AbonoId { get; set; }
    public Abono? Abono { get; set; }

    /// <summary>Cuántos dueños tiene asignados (debe ser exactamente 1).</summary>
    public int ContarDuenos() =>
        (ClienteId.HasValue ? 1 : 0)
        + (VehiculoId.HasValue ? 1 : 0)
        + (AbonoId.HasValue ? 1 : 0);
}

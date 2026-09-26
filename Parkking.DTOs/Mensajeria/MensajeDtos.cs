using System.Text.Json.Serialization;

namespace Parkking.DTOs.Mensajeria;

public class EnviarReciboEmailRequest
{
    /// <summary>Opcional: fuerza el destinatario (si no, Abono.Email ?? Cliente.Email).</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

/// <summary>
/// Destino del reporte: email de avisos del estacionamiento, o el mail de un usuario del tenant.
/// </summary>
public class EnviarReporteEmailRequest
{
    /// <summary>"estacionamiento" | "usuario"</summary>
    public string Destino { get; set; } = DestinoReporteEmail.Estacionamiento;

    /// <summary>Obligatorio si Destino = usuario.</summary>
    public int? UsuarioId { get; set; }

    /// <summary>Solo pagos.xlsx.</summary>
    public DateOnly? Desde { get; set; }

    /// <summary>Solo pagos.xlsx.</summary>
    public DateOnly? Hasta { get; set; }
}

public static class DestinoReporteEmail
{
    public const string Estacionamiento = "estacionamiento";
    public const string Usuario = "usuario";
}

public class MensajeDto
{
    public int MensajeId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Destinatario { get; set; } = string.Empty;
    public string? Remitente { get; set; }
    public string Asunto { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime Fecha { get; set; }
    public int? ReciboId { get; set; }
    public int? ClienteId { get; set; }
    public int? AbonoId { get; set; }
    public bool Simulado { get; set; }
}

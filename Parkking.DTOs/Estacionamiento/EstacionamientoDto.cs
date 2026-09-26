namespace Parkking.DTOs.Estacionamiento;

public class DatosEstacionamientoDto
{
    public int EstacionamientoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? LocadorNombre { get; set; }
    public string? LocadorDocumento { get; set; }
    public string? LocadorDomicilio { get; set; }
    public int DiaVencimientoAbono { get; set; }
    public bool AplicaRecargo { get; set; }
    public decimal PorcentajeRecargo { get; set; }
    public bool Activo { get; set; } = true;
    public bool ImprimirReciboAlCobrar { get; set; } = true;

    /// <summary>Si true, al cobrar se envía el recibo por email al cliente.</summary>
    public bool EnviarReciboPorEmail { get; set; }

    /// <summary>Cláusula de seguro obligatorio en el PDF de contrato.</summary>
    public bool ContratoSeguroObligatorio { get; set; } = true;

    /// <summary>Plazo del contrato en meses (solo PDF; no afecta vigencia del abono).</summary>
    public int ContratoPlazoMeses { get; set; } = 12;

    /// <summary>Si true, al crear un abono se descarga automáticamente el PDF de contrato.</summary>
    public bool GenerarContratoAlCrearAbono { get; set; } = true;

    /// <summary>Email de avisos del estacionamiento (obligatorio).</summary>
    public string EmailAvisos { get; set; } = string.Empty;
}

/// <summary>Alias de compatibilidad con el front actual.</summary>
public class EstacionamientoDto : DatosEstacionamientoDto
{
}

namespace Parkking.DTOs.Documentos;

public class DocumentoDto
{
    public int DocumentoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string NombreOriginal { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public string? Observacion { get; set; }
    public DateTime FechaCarga { get; set; }

    /// <summary>Cliente | Vehiculo | Abono</summary>
    public string DuenoTipo { get; set; } = string.Empty;
    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public int? VehiculoId { get; set; }
    public string? VehiculoPatente { get; set; }
    public int? AbonoId { get; set; }

    /// <summary>Etiqueta para agrupar en UI del abono.</summary>
    public string GrupoLabel { get; set; } = string.Empty;
}

public class DocumentoAbonoVistaDto
{
    public int AbonoId { get; set; }
    public List<DocumentoDto> Documentos { get; set; } = new();
}

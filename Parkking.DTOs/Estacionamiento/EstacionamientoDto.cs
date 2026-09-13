namespace Parkking.DTOs.Estacionamiento;

public class DatosEstacionamientoDto
{
    public int EstacionamientoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public int DiaVencimientoAbono { get; set; }
    public bool AplicaRecargo { get; set; }
    public decimal PorcentajeRecargo { get; set; }
    public int? DiasUmbralProporcional { get; set; }
    public bool Activo { get; set; } = true;
    public bool ImprimirReciboAlCobrar { get; set; } = true;
}

/// <summary>Alias de compatibilidad con el front actual.</summary>
public class EstacionamientoDto : DatosEstacionamientoDto
{
}

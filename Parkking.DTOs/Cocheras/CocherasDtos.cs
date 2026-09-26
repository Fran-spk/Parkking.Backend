using Parkking.DTOs.Shared;
using Parkking.Models;

namespace Parkking.DTOs.Cocheras;

public class CocheraRequest
{
    public string Numero { get; set; } = string.Empty;
    public int CategoriaCocheraId { get; set; }
    public EstadoCochera EstadoCochera { get; set; }
    public string? Observacion { get; set; }
    public bool MultipleOcupacion { get; set; }
    /// <summary>Obligatorio si MultipleOcupacion; cupo máximo de abonos activos (≥ 2).</summary>
    public int? MaxOcupacion { get; set; }
    public List<int>? VehiculosPermitidosIds { get; set; }
}

public class CocheraResponseDto
{
    public int CocheraId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int CategoriaCocheraId { get; set; }
    public EstadoCochera EstadoCochera { get; set; }
    public string? Observacion { get; set; }
    public bool MultipleOcupacion { get; set; }
    public int? MaxOcupacion { get; set; }
    /// <summary>Abonos activos que hoy usan esta plaza.</summary>
    public int AbonosActivos { get; set; }
    /// <summary>Cupo efectivo (1 sin multi; MaxOcupacion con multi).</summary>
    public int CapacidadMaxima { get; set; }
    public bool EstaDisponible { get; set; }
    public List<int> VehiculosPermitidosIds { get; set; } = new();
    public CategoriaCocheraResumenDto? CategoriaCochera { get; set; }
}

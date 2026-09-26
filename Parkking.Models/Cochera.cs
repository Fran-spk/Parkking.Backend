using System.ComponentModel.DataAnnotations;

namespace Parkking.Models;

public class Cochera : IMultiTenant
{
    public int CocheraId { get; set; }
    public int EstacionamientoId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int CategoriaCocheraId { get; set; }
    public CategoriaCochera CategoriaCochera { get; set; } = null!;
    public EstadoCochera EstadoCochera { get; set; }
    public string? Observacion { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<TipoVehiculo> VehiculosPermitidos { get; set; } = new List<TipoVehiculo>();
    public ICollection<AbonoPlaza> Plazas { get; set; } = new List<AbonoPlaza>();
    public bool MultipleOcupacion { get; set; }

    /// <summary>
    /// Cupo máximo de abonos activos cuando <see cref="MultipleOcupacion"/> es true.
    /// Null o ≤1 con multi = se trata como 2. Sin multi el cupo es siempre 1.
    /// </summary>
    public int? MaxOcupacion { get; set; }

    public int ContarAbonosActivos() =>
        Plazas.Count(p => p.Activo && p.Abono != null && p.Abono.Activo);

    /// <summary>Cupo efectivo: 1 sin multi; con multi usa MaxOcupacion (mínimo 2).</summary>
    public int CapacidadMaxima()
    {
        if (!MultipleOcupacion)
            return 1;
        var max = MaxOcupacion ?? 2;
        return max < 2 ? 2 : max;
    }

    public bool EstaDisponible()
    {
        if (EstadoCochera != EstadoCochera.Habilitada)
            return false;

        return ContarAbonosActivos() < CapacidadMaxima();
    }
}

public enum EstadoCochera
{
    Habilitada,
    UsoInterno
}

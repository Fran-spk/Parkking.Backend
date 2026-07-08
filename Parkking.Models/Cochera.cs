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
    public ICollection<AbonoCochera> Abonos { get; set; } = new List<AbonoCochera>();
    public bool MultipleOcupacion { get; set; }

    public bool EstaDisponible()
    {
        if (EstadoCochera != EstadoCochera.Habilitada)
            return false;

        var tieneAbonosActivos = Abonos.Any(a => a.Activo);
        if (!tieneAbonosActivos)
            return true;

        return MultipleOcupacion;
    }
}

public enum EstadoCochera
{
    Habilitada,
    UsoInterno
}

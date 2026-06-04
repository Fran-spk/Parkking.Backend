using MODELO;
using Parkking_backend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Cochera
{
    public int CocheraId { get; set; }

    public int EstacionamientoId { get; set; }
    public Estacionamiento Estacionamiento { get; set; }

    public string Numero { get; set; }

    public int CategoriaCocheraId { get; set; }
    public CategoriaCochera CategoriaCochera { get; set; }

    public EstadoCochera EstadoCochera { get; set; }

    public string? Observacion { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<TipoVehiculo> VehiculosPermitidos { get; set; } = new List<TipoVehiculo>();

    public ICollection<AbonoCochera> Abonos { get; set; } = new List<AbonoCochera>();

    public bool MultipleOcupacion { get; set; }

    public bool EstaDisponible()
    {
        if (EstadoCochera != EstadoCochera.Habilitada)
        {
            return false;
        }

        var tieneAbonosActivos = Abonos.Any(a => a.Activo);

        if (!tieneAbonosActivos)
            return true;

        if (!MultipleOcupacion)
            return false;
        return true;
    }

}


public enum EstadoCochera
{
    Habilitada,
    UsoInterno
}
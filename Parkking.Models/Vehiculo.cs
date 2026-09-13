using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models;

/// <summary>
/// Vehículo con identidad propia, perteneciente a un cliente.
/// </summary>
public class Vehiculo : IMultiTenant
{
    [Key]
    public int VehiculoId { get; set; }

    [ForeignKey(nameof(Estacionamiento))]
    public int EstacionamientoId { get; set; }
    public DatosEstacionamiento? Estacionamiento { get; set; }

    [ForeignKey(nameof(Cliente))]
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string Patente { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ModeloVehiculo { get; set; }

    [ForeignKey(nameof(TipoVehiculo))]
    public int TipoVehiculoId { get; set; }
    public TipoVehiculo TipoVehiculo { get; set; } = null!;

    public bool Activo { get; set; } = true;

    /// <summary>
    /// Vínculo al abono (1 vehículo → 1 abono).
    /// </summary>
    public AbonoVehiculo? AbonoVehiculo { get; set; }
}

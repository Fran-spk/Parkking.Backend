using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models;

/// <summary>
/// Participación de un vehículo en un abono (modalidad fijo/flexible).
/// </summary>
public class AbonoVehiculo : IMultiTenant
{
    [Key]
    public int AbonoVehiculoId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Abono))]
    public int AbonoId { get; set; }
    public Abono Abono { get; set; } = null!;

    [ForeignKey(nameof(Vehiculo))]
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;

    [Required]
    public ModalidadVehiculoAbono Modalidad { get; set; }

    /// <summary>
    /// Solo para modalidad Fijo. Debe ser una plaza del mismo abono.
    /// </summary>
    [ForeignKey(nameof(AbonoPlaza))]
    public int? AbonoPlazaId { get; set; }
    public AbonoPlaza? AbonoPlaza { get; set; }
}

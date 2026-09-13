using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models;

/// <summary>
/// Cochera incluida en el conjunto contratado de un abono.
/// </summary>
public class AbonoPlaza : IMultiTenant
{
    [Key]
    public int AbonoPlazaId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Abono))]
    public int AbonoId { get; set; }
    public Abono Abono { get; set; } = null!;

    [ForeignKey(nameof(Cochera))]
    public int CocheraId { get; set; }
    public Cochera Cochera { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public ICollection<AbonoVehiculo> VehiculosFijos { get; set; } = new List<AbonoVehiculo>();
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models;

public class MetodoDePago : IMultiTenant
{
    [Key]
    public int MetodoDePagoId { get; set; }

    [Required]
    [ForeignKey(nameof(Estacionamiento))]
    public int EstacionamientoId { get; set; }
    public DatosEstacionamiento Estacionamiento { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public override string ToString() => Nombre;
}

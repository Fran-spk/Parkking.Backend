using System.ComponentModel.DataAnnotations;

namespace Parkking.Models.Finanzas;

public class TipoGasto : IMultiTenant
{
    [Key]
    public int TipoGastoId { get; set; }

    public int EstacionamientoId { get; set; }

    [Required]
    [MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
    public ICollection<ReglaAsignacion> Reglas { get; set; } = new List<ReglaAsignacion>();
}

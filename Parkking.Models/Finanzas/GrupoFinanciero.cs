using System.ComponentModel.DataAnnotations;

namespace Parkking.Models.Finanzas;

public class GrupoFinanciero : IMultiTenant
{
    [Key]
    public int GrupoFinancieroId { get; set; }

    public int EstacionamientoId { get; set; }

    [Required]
    [MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<MovimientoGrupoFinanciero> Movimientos { get; set; } = new List<MovimientoGrupoFinanciero>();
    public ICollection<ReglaAsignacion> Reglas { get; set; } = new List<ReglaAsignacion>();
}

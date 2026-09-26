using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models.Finanzas;

public class MovimientoGrupoFinanciero : IMultiTenant
{
    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Movimiento))]
    public int MovimientoId { get; set; }
    public Movimiento Movimiento { get; set; } = null!;

    [ForeignKey(nameof(GrupoFinanciero))]
    public int GrupoFinancieroId { get; set; }
    public GrupoFinanciero GrupoFinanciero { get; set; } = null!;
}

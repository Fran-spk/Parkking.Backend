using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Models.Finanzas;

/// <summary>Un criterio (cliente o tipo de gasto) hacia un grupo. Pueden convivir varias.</summary>
public class ReglaAsignacion : IMultiTenant
{
    [Key]
    public int ReglaAsignacionId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(GrupoFinanciero))]
    public int GrupoFinancieroId { get; set; }
    public GrupoFinanciero GrupoFinanciero { get; set; } = null!;

    public CriterioRegla Criterio { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? TipoGastoId { get; set; }
    public TipoGasto? TipoGasto { get; set; }

    public bool Activa { get; set; } = true;
}

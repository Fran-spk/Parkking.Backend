namespace Parkking.DTOs.Finanzas;

public class ReglaAsignacionDto
{
    public int ReglaAsignacionId { get; set; }
    public int GrupoFinancieroId { get; set; }
    public string GrupoNombre { get; set; } = string.Empty;
    public int Criterio { get; set; }
    public string CriterioDescripcion { get; set; } = string.Empty;
    public int? ClienteId { get; set; }
    public int? TipoGastoId { get; set; }
    public bool Activa { get; set; }
}

public class CrearReglaAsignacionRequest
{
    public int GrupoFinancieroId { get; set; }
    public int Criterio { get; set; }
    public int? ClienteId { get; set; }
    public int? TipoGastoId { get; set; }
}

public class EditarReglaAsignacionRequest
{
    public int GrupoFinancieroId { get; set; }
    public int Criterio { get; set; }
    public int? ClienteId { get; set; }
    public int? TipoGastoId { get; set; }
}

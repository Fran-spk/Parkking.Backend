namespace Parkking.DTOs.Finanzas;

public class GrupoFinancieroDto
{
    public int GrupoFinancieroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

public class CrearGrupoFinancieroRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class EditarGrupoFinancieroRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

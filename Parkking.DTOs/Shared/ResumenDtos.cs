namespace Parkking.DTOs.Shared;

public class TipoVehiculoResumenDto
{
    public int TipoVehiculoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CategoriaCocheraResumenDto
{
    public int CategoriaCocheraId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CocheraResumenDto
{
    public int CocheraId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int CategoriaCocheraId { get; set; }
    public CategoriaCocheraResumenDto? CategoriaCochera { get; set; }
}

public class ClienteResumenDto
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? Domicilio { get; set; }
    public string? Telefono { get; set; }
    /// <summary>Opcional.</summary>
    public string? Email { get; set; }
}

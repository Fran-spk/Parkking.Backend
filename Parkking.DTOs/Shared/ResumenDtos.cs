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
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}

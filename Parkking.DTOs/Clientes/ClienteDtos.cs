using Parkking.DTOs.Abonos;

namespace Parkking.DTOs.Clientes;

public class ClienteRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Observacion { get; set; }
}

public class ClienteDto
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Observacion { get; set; }
    public bool Activo { get; set; }
    public List<AbonoCocheraDto>? Abonos { get; set; }
}

namespace Parkking.DTOs.Usuario;

public class UsuarioDto
{
    public string UsuarioName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
}

public class EditarUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
}

public class CambiarContraseñaRequest
{
    public string passwordActual { get; set; } = string.Empty;
    public string passwordNueva { get; set; } = string.Empty;
}
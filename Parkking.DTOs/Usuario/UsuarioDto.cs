namespace Parkking.DTOs.Usuario;

public class UsuarioDto
{
    public int UsuarioId { get; set; }
    public string UsuarioName { get; set; } = string.Empty;
    /// <summary>Email del usuario (obligatorio).</summary>
    public string Mail { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
}

/// <summary>Usuario del estacionamiento actual (para pickers, p. ej. envío de reportes).</summary>
public class UsuarioResumenDto
{
    public int UsuarioId { get; set; }
    public string UsuarioName { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Email del usuario (obligatorio).</summary>
    public string Mail { get; set; } = string.Empty;
}

public class EditarUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Email del usuario (obligatorio).</summary>
    public string Mail { get; set; } = string.Empty;
    public string? Telefono { get; set; }
}

public class CambiarContraseñaRequest
{
    public string passwordActual { get; set; } = string.Empty;
    public string passwordNueva { get; set; } = string.Empty;
}

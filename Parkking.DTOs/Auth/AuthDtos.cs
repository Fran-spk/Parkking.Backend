namespace Parkking.DTOs.Auth;

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginUsuarioDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Email del usuario (obligatorio).</summary>
    public string Mail { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Mensaje { get; set; } = "Login exitoso";
    public LoginUsuarioDto Usuario { get; set; } = new();
}

public class AuthMeDto
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    /// <summary>Email del usuario (claim JWT).</summary>
    public string? Email { get; set; }
}

public class EstacionamientoUsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    /// <summary>Email de avisos del estacionamiento (obligatorio).</summary>
    public string EmailAvisos { get; set; } = string.Empty;
}

public class SeleccionarEstacionamientoRequest
{
    public int EstacionamientoId { get; set; }
}
